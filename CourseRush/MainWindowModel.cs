using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Controls;
using CourseRush.Core;
using CourseRush.Core.Util;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Resultful;

namespace CourseRush
{
    public delegate void AutoFontSizeChanged(double fontSizeFactor);
    public sealed class MainWindowModel<TUniversity, TCourse, TError, TCourseCategory, TCourseSelection> :
        IMainWindowModel where TUniversity : IUniversity<TCourse, TError, TCourseSelection>
        where TCourse : ICourse
        where TCourseSelection : class, ICourseSelection
        where TError : BasicError
        where TCourseCategory : ICourseCategory
    {
        
        private event AutoFontSizeChanged? AutoFontSizeChanged;
        private event Action<IUserInfo>? UserInfoUpdated;
        private event Action<ICourseSelection>? SessionSelected;

        private readonly ISessionClient<TError, TCourseSelection, TCourse, TCourseCategory> _sessionClient;
        private readonly TUniversity _university;

        private readonly CourseSelectionListPageModel<TCourse, TCourseCategory> _courseSelectionListPageModel;
        private readonly SelectionSessionPageModel<TCourseSelection> _selectionSessionsPageModel;

        private readonly CurrentCourseTablePage _currentCourseTablePage = new();
        private readonly CourseSelectionListPage _courseSelectionListPage;
        private readonly SelectionSessionsPage _selectionSessionsPage;
        
        private Option<TCourseSelection> _currentSelection;
        private Option<IReadOnlyList<TCourseCategory>> _categoryCache;
        private Option<ICourseSelectionClient<TError, TCourse, TCourseCategory>> _selectionClientCache;

        public MainWindowModel(TUniversity university, ISessionClient<TError, TCourseSelection, TCourse, TCourseCategory> sessionClient)
        {
            _university = university;
            _sessionClient = sessionClient;
            _selectionSessionsPageModel = new SelectionSessionPageModel<TCourseSelection>(_university.SelectionPresentedData, OnSessionSelected);
            _selectionSessionsPage = new SelectionSessionsPage(_selectionSessionsPageModel, ReloadSession, RegisterFontSizeChanged);
            _courseSelectionListPageModel = new CourseSelectionListPageModel<TCourse, TCourseCategory>(_university.CoursePresentedData, ReloadCourse, LoadCourses);
            _courseSelectionListPage = new CourseSelectionListPage(_courseSelectionListPageModel,  RegisterFontSizeChanged);
        }

        private void ReloadCourse(TCourseCategory category, Action<IReadOnlyList<TCourse>> reloadAction)
        {
            Task.Run(() =>
            {
                _selectionClientCache.Tee(client => client.GetCoursesByCategory(category).Tee(reloadAction).TeeError(error => Growl.Error(error.Message)));
            });
        }

        private void RegisterFontSizeChanged(AutoFontSizeChanged handler)
        {
            AutoFontSizeChanged += handler;
        }

        private void LoadCourses(Stream stream, Action<IReadOnlyList<TCourse>> acceptor)
        {
            Task.Run(() =>
            {
                try
                {
                    (JsonNode.Parse(stream) as JsonArray)?
                        .Where(node => node is JsonObject)
                        .Select(node => _university.CourseCodec.FromJson(node!.AsObject()))
                        .ToList()
                        .CombineResults(_university.CourseCodec.ErrorCombinator)
                        .TeeError(err => Growl.Error(err.Message)).Tee(acceptor);
                }
                catch (JsonException e)
                {
                    Growl.Error(e.Message);
                }
            });
        }

        private void ReloadSession()
        {
            Task.Run(() => _sessionClient.GetOngoingCourseSelections().Tee(list => _selectionSessionsPage?.Invoke(() =>
            {
                _selectionSessionsPageModel.Clear();
                list.Do(_selectionSessionsPageModel.AddSelection);
            })));
        }

        private void OnSessionSelected(TCourseSelection selection)
        {
            _currentSelection = selection;
            Task.Run(() =>
            {
                _selectionClientCache = _sessionClient.GetSelectionClient(selection).ToOption();
                _selectionClientCache
                    .Tee(client => client.GetCategoriesInRound().TeeError(error => Growl.Error(error.Message))
                        .Tee(list => _categoryCache = list.ToOption())
                        .Tee(list => _courseSelectionListPage.Invoke(()=>_courseSelectionListPageModel.UpdateCategories(list))));
                SessionSelected?.Invoke(selection);
            });
        }

        public void RegisterSessionSelectedListener(Action<ICourseSelection> action)
        {
            SessionSelected += action;
        }

        public void ReloadUserInfo()
        {
            _sessionClient.GetUserInfo().Tee(info =>
            {
                UserInfoUpdated?.Invoke(info);
                _courseSelectionListPage.UserInfo = info;
            }).TeeError(error => Growl.Error(error.Message));
        }

        public Page GetCurrentCourseTablePage()
        {
            return _currentCourseTablePage;
        }

        public Page GetCourseSelectionListPage()
        {
            return _courseSelectionListPage;
        }

        public Page GetSelectionSessionsPage()
        {
            return _selectionSessionsPage;
        }

        public void OnAutoFontSizeChanged(double fontSizeFactor)
        {
            AutoFontSizeChanged?.Invoke(fontSizeFactor);
        }

        public void RegisterUserInfoListener(Action<IUserInfo> userInfoAction)
        {
            UserInfoUpdated += userInfoAction;
        }
    }
}