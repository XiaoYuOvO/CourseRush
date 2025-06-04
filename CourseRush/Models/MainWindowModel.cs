using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CourseRush.Auth;
using CourseRush.Core;
using CourseRush.Core.Task;
using CourseRush.Core.Util;
using CourseRush.Pages;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using Resultful;
using MessageBox = HandyControl.Controls.MessageBox;
using MessageBoxInfo = HandyControl.Data.MessageBoxInfo;
using TaskStatus = CourseRush.Core.Task.TaskStatus;
using Utils = CourseRush.Core.Util.Utils;

namespace CourseRush.Models;

public delegate void AutoFontSizeChanged(double fontSizeFactor);
public sealed class MainWindowModel<TUniversity, TCourse, TSelectedCourse, TError, TCourseCategory, TCourseSelection, TSessionClient> :
    IMainWindowModel where TUniversity : IUniversity<TCourse, TError, TCourseSelection, TSessionClient>
    where TCourse : class, ICourse, IPresentedDataProvider<TCourse>, IJsonSerializable<TCourse, TError>
    where TCourseSelection : class, ISelectionSession, IPresentedDataProvider<TCourseSelection>, IJsonSerializable<TCourseSelection, TError>
    where TError : BasicError, ICombinableError<TError>, ISelectionError
    where TCourseCategory : ICourseCategory
    where TSelectedCourse : class, TCourse, ISelectedCourse, IPresentedDataProvider<TSelectedCourse>, IJsonSerializable<TSelectedCourse, TError>
    where TSessionClient : ISessionClient<TError, TCourseSelection, TCourse, TSelectedCourse, TCourseCategory>
{
        
    private event AutoFontSizeChanged? AutoFontSizeChanged;
    private event Action<IUserInfo>? UserInfoUpdated;
    private event Action<ISelectionSession>? SessionSelected;

    private ISessionClient<TError, TCourseSelection, TCourse, TSelectedCourse, TCourseCategory>? _sessionClient;
    private readonly TUniversity _university;
    private readonly UsernamePassword _profile;

    private readonly CourseSelectionListPageModel<TCourse, TCourseCategory> _courseSelectionListPageModel;
    private readonly SelectionSessionPageModel<TCourseSelection, TError> _selectionSessionsPageModel;
    private readonly CourseSelectionQueuePageModel<TError, TCourse> _selectionQueuePageModel;
    private readonly CurrentCourseTablePageModel<TSelectedCourse, TError> _currentCourseTablePageModel;

    private readonly CourseSelectionListPage _courseSelectionListPage;
    private readonly CourseSelectionQueuePage _selectionQueuePage;
    private readonly SelectionSessionsPage _selectionSessionsPage;
    private readonly CurrentCourseTablePage _currentCourseTablePage;

    private Option<TCourseSelection> _currentSelection = Option<TCourseSelection>.None;
    private Option<ICourseSelectionClient<TError, TCourse, TSelectedCourse, TCourseCategory>> _selectionClientCache;

    public MainWindowModel(TUniversity university, UsernamePassword profile, ISessionClient<TError, TCourseSelection, TCourse, TSelectedCourse, TCourseCategory> sessionClient)
    {
        _university = university;
        _profile = profile;
        _sessionClient = sessionClient;
            
        _selectionSessionsPageModel = new SelectionSessionPageModel<TCourseSelection, TError>(TCourseSelection.GetPresentedData());
        _selectionSessionsPageModel.OnSessionSelected += OnSessionSelected;
        _selectionSessionsPage = new SelectionSessionsPage(_selectionSessionsPageModel, ReloadSession, RegisterFontSizeChanged);

        _courseSelectionListPageModel = new CourseSelectionListPageModel<TCourse, TCourseCategory>(ReloadCourse, LoadCoursesWithConflictCheck, SelectCourses);
        _courseSelectionListPage = new CourseSelectionListPage(_courseSelectionListPageModel, RegisterFontSizeChanged);

        _selectionQueuePageModel = new CourseSelectionQueuePageModel<TError, TCourse>(RegisterFontSizeChanged, SubmitTask);
        _selectionQueuePage = new CourseSelectionQueuePage(_selectionQueuePageModel, RegisterFontSizeChanged);
            
        _currentCourseTablePageModel = new CurrentCourseTablePageModel<TSelectedCourse, TError>(RemoveCourseSelection);
        _currentCourseTablePage = new CurrentCourseTablePage(_currentCourseTablePageModel, ReloadCourseTable);
        _currentCourseTablePage.RegisterAutoFontSize(RegisterFontSizeChanged);
        _currentCourseTablePageModel.RegisterAutoFontSize(RegisterFontSizeChanged);
        // SetupOnlineChecker();
    }

    private void SetupOnlineChecker()
    {
        new Thread(() =>
        {
            while (!Environment.HasShutdownStarted)
            {
                _sessionClient?.IsOnline().Tee(isOnline =>
                {
                    if (isOnline) return;
                    var dialog = _selectionSessionsPage.Invoke(() => Dialog.Show<ReloginDialog>("MainWindow"));
                    _university.LoginAndCreateClient(_profile).Tee(client => {
                        _sessionClient = client;
                        _currentSelection.Tee(OnSessionSelected);
                        dialog.Invoke(dialog.Close);
                    });
                }).TeeError(e => Growl.Error(e.Message));
                Thread.Sleep(1000);
            }
        })
        {
            Name = $"CourseRush-{_university.Name}-OnlineChecker",
            IsBackground = true
        }.Start();
    }

    private async Task ReloadCourse(TCourseCategory category, Action<IEnumerable<TCourse>> reloadAction)
    {
        await _selectionClientCache.TeeAsync(async client =>
        {
            await foreach (var results in client.GetCoursesByCategory(category).ConfigureAwait(false))
            {
                //TODO Course buffer
                results.CombineResults().Tee(reloadAction).TeeError(error => Growl.Error(error.Message));
            }
        });
    }

    private void CheckForCourseConflict(TCourse course)
    {
        _currentCourseTablePageModel.ResolveCourseConflictWithCurrent(course);
        _selectionQueuePageModel.ApplyCourseConflict(course);
    }

    private void CheckForCoursesConflict(IReadOnlyList<TCourse> courses)
    {
        courses.AsParallel().ForAll(CheckForCourseConflict);
    }

    private void RegisterFontSizeChanged(AutoFontSizeChanged handler)
    {
        AutoFontSizeChanged += handler;
    }

    private void LoadCoursesWithConflictCheck(Stream stream, Action<IReadOnlyList<TCourse>> acceptor)
    {
        Task.Run(() =>
        {
            try
            {
                (JsonNode.Parse(stream) as JsonArray)?
                    .Where(node => node is JsonObject)
                    .Select(node => TCourse.FromJson(node!.AsObject()))
                    .ToList()
                    .CombineResults()
                    .TeeError(err => Growl.Error(err.Message))
                    .Tee(CheckForCoursesConflict).Tee(acceptor);
            }
            catch (JsonException e)
            {
                Growl.Error(e.Message);
            }
        });
    }

    private void ReloadSession()
    {
        Task.Run(() => _sessionClient?.GetOngoingCourseSelections().Tee(list => _selectionSessionsPage.Invoke(() =>
        {
            _selectionSessionsPageModel.Clear();
            list.Do(_selectionSessionsPageModel.AddSelection);
        })).TeeError(e => Growl.Error(e.Message)));
    }

    private async void OnSessionSelected(TCourseSelection selection)
    {
        _currentSelection = selection;
        _selectionClientCache = _sessionClient?.GetSelectionClient(selection).ToOption() ?? Option<ICourseSelectionClient<TError, TCourse, TSelectedCourse, TCourseCategory>>.None;
        await _selectionClientCache
            .TeeAsync(async client => await Task.Run(() =>
                client.GetWeekTimeTable()
                    .Tee(table => _currentCourseTablePage.Invoke(() => _currentCourseTablePageModel.UpdateTimeTable(table)))
                    .TeeError(error => Growl.Error(error.Message))))
            .TeeAsync(async client => await Task.Run(() => client.GetCategoriesInRound()
                .TeeError(error => Growl.Error(error.Message))
                .TeeAsync(async list => await Task.Run(() => _courseSelectionListPage.Invoke(() => _courseSelectionListPageModel.UpdateCategories(list))))))
            .Tee(client => _selectionQueuePageModel.SetClient(client))
            .TeeAsync(async _ => await ReloadCourseTable())
            .ContinueWith(task => task.Exception.ToOption().Tee(ex => Growl.Error(ex!.Message)));
        SessionSelected?.Invoke(selection);
    }

    private void SelectCourses(IReadOnlyList<TCourse> courses)
    {
        //TODO Selected courses conflicts with each other
        _selectionClientCache.Tee(_ =>
        {
            var conflictedCourses = courses.Where(course => course.ConflictsCache.Count != 0).ToList();
            if (conflictedCourses.Count != 0)
            {
                var messageBoxText = string.Join("\n",
                    conflictedCourses.Select(course =>
                        $"{string.Format(Language.ui_message_course_conflict, course.ToSimpleString())}" +
                        $"{"\n    " + 
                           string.Join("\n    ", 
                               course.ConflictsCache.Select(pair => 
                                   string.Format(Language.ui_message_course_conflict_info, pair.Key.ToSimpleString(), 
                                       "\n          " + 
                                       string.Join("\n          ", 
                                           pair.Value.Select(result => 
                                               string.Format(Language.ui_message_course_conflict_info_week, 
                                                   string.Join(",", CollectionUtils.FindRanges(result.ConflictWeeks).Select(Utils.ToSimpleString))) + 
                                               string.Join("#", result.ConflictMap.Select(weekPair => 
                                                   CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(weekPair.Key) + 
                                                   string.Format(Language.ui_message_course_conflict_info_lesson, 
                                                       string.Join(",", CollectionUtils.FindRanges(weekPair.Value.ToImmutableList()).Select(Utils.ToSimpleString))))))))))}"));
                if (MessageBox.Show(new MessageBoxInfo
                    {
                        Message =
                            $"{messageBoxText}\n{Language.ui_message_course_conflict_solve_by_remove}",
                        Button = MessageBoxButton.YesNo,
                        IconBrushKey = ResourceToken.AccentBrush, IconKey = ResourceToken.WarningGeometry
                    }) == MessageBoxResult.Yes)
                {
                    var taskCount = 0;
                    SubmitTask(new ParallelSelectionTask<TError, TCourse>(conflictedCourses
                        .Select(targetCourse =>
                        {
                            List<SelectionTask<TError, TCourse>> tasks = [];
                            foreach (var (course, _) in targetCourse.ConflictsCache)
                            {
                                if (course is TCourse t) tasks.Add(new RemoveSelectionTask<TError, TCourse>(t, _selectionQueuePageModel));
                            }

                            tasks.Add(new SubmitSelectionTask<TError, TCourse>(targetCourse));
                            taskCount += tasks.Count;
                            return new SequentialSelectionTask<TError, TCourse>(tasks, _selectionQueuePageModel);
                        }).ToList(), _selectionQueuePageModel), taskCount);
                }
            }

            CreateTaskFromCourses(courses.Where(course => course.ConflictsCache.Count == 0).ToList()).Tee(task => SubmitTask(task, courses.Count));
        });
    }

    private void SubmitTask(SelectionTask<TError, TCourse> task, int taskCount, string infoMessage)
    {
        SubmitTask(task);
        Growl.Info(string.Format(infoMessage, taskCount));
    }

    private void SubmitTask(SelectionTask<TError, TCourse> task, int taskCount)
    {
        SubmitTask(task, taskCount, Language.ui_message_course_selected);
    }

    private void SubmitTask(SelectionTask<TError, TCourse> task)
    {
        task.Logger = _selectionQueuePageModel;
        task.StatusChanged += status => OnSelectionTaskStatusUpdate(task, status);
        _courseSelectionListPageModel.CheckAllCoursesConflicts(task.ApplyCourseConflict);
        _selectionQueuePageModel.SubmitTask(task).TeeError(err => Growl.Error(err.Message));
    }

    private void RemoveCourseSelection(TSelectedCourse course)
    {
        SubmitTask(new RemoveSelectionTask<TError, TCourse>(course, _selectionQueuePageModel), 1, Language.ui_message_course_remove_submitted);
    }

    private void OnSelectionTaskStatusUpdate(SelectionTask<TError, TCourse> selectionTask, TaskStatus taskStatus)
    {
        if (taskStatus != TaskStatus.Completed && taskStatus != TaskStatus.Failed && taskStatus != TaskStatus.Cancelled) return;
        OnSelectionTaskFinished(selectionTask, taskStatus);
        if (taskStatus == TaskStatus.Completed)
        {
            Growl.Info($"{selectionTask.Name}\n{taskStatus.LocalizedName}");
        }else if (taskStatus == TaskStatus.Failed)
        {
            Growl.Error($"{selectionTask.Name}\n{taskStatus.LocalizedName}");
        }
    }

    private void OnSelectionTaskFinished(SelectionTask<TError, TCourse> task,TaskStatus status)
    {
        //Run on task thread!
        if (status == TaskStatus.Completed)
        {
            _ = ReloadCourseTable();
            Task.Run(() => _courseSelectionListPageModel.CheckAllCoursesConflicts(task.ApplyPostSelectionCourseConflict));
        }
        else
        {
            Task.Run(() => _courseSelectionListPageModel.CheckAllCoursesConflicts(task.UndoCourseConflict));
        }
    }

    private Option<SelectionTask<TError, TCourse>> CreateTaskFromCourses(IReadOnlyList<TCourse> courses)
    {
        if (courses.Count == 0) return Option<SelectionTask<TError, TCourse>>.None;
        if (courses.Count == 1)
        {
            var submitSelectionTask = new SubmitSelectionTask<TError, TCourse>(courses[0]);
            return submitSelectionTask;
        }
        if (MessageBox.Show(new MessageBoxInfo
            {
                Message = $"{Language.ui_message_choose_multi_select_mode}\n{Language.ui_message_choose_multi_select_mode_2}",
                Button = MessageBoxButton.YesNo,
                IconBrushKey = ResourceToken.InfoBrush, IconKey = ResourceToken.AskGeometry
            }) == MessageBoxResult.Yes)
        {
            return PrioritizedCourseSelectionReorderWindow<TCourse>.ShowReorderWindow(courses).Map(list =>
                new PrioritizedSelectionTask<TError, TCourse>(list
                    .Select(course => new SubmitSelectionTask<TError, TCourse>(course))
                    .ToImmutableList(), _selectionQueuePageModel) as SelectionTask<TError, TCourse>);
        }

        return new ParallelSelectionTask<TError, TCourse>(courses.Select(course => new SubmitSelectionTask<TError, TCourse>(course)).ToList());
    }

    private async Task ReloadCourseTable()
    {
        await _selectionClientCache.TeeAsync(async client =>
        {
            await Task.Run(client.GetCurrentCourseTable).Tee(list => {
                _currentCourseTablePage.Invoke(() =>
                {
                    _currentCourseTablePageModel.ClearCourses();
                    if (list.Count != 0) 
                        _currentCourseTablePageModel.AddCourses(list);
                });
            }).TeeError(err => Growl.Error(err.Message));
        });
    }

    public void RegisterSessionSelectedListener(Action<ISelectionSession> action)
    {
        SessionSelected += action;
    }

    public async void ReloadUserInfo()
    {
        var userInfo = await Task.Run(() => _sessionClient?.GetUserInfo());
        userInfo?.Tee(info =>
        {
            UserInfoUpdated?.Invoke(info);
            _courseSelectionListPage.UserInfo = info;
        }).TeeError(error => Growl.Error(error.Message));
    }

    public string GetUserAccount()
    {
        return _profile.Username;
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

    public Page GetSelectionQueuePage()
    {
        return _selectionQueuePage;
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