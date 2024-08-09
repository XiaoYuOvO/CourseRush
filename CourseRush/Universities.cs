using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CourseRush.Auth;
using CourseRush.Auth.HNU;
using CourseRush.Auth.HNU.Hdjw;
using CourseRush.Core;
using CourseRush.Core.Network;
using CourseRush.HNU;
using CourseRush.HNU.Debug;
using Resultful;

namespace CourseRush;

public interface IMainWindowModelProvider
{
    public Result<Func<IMainWindowModel>, BasicError> LoginAndCreateMainWindowModel(UsernamePassword usernamePassword);
}

public interface IUniversity<TCourse, TError, TCourseSelection> where TCourse : ICourse where TCourseSelection : ICourseSelection where TError : BasicError{
    public List<PresentedData<TCourse>> CoursePresentedData { get; }
    public List<PresentedData<TCourseSelection>> SelectionPresentedData { get; }
    public Codec<TCourse, TError> CourseCodec { get; }
    public Codec<TCourseSelection, TError> SelectionCodec { get; }
}

[SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeNotEvident")]
public static class Universities
{
    private static readonly Dictionary<string, IMainWindowModelProvider> UniversityRegistry = new();

    public static readonly
        UniversityProperty<HdjwError, HNUCourse, HNUCourseSelection, HNUCourseCategory, HdjwAuthResult>
        HNU =
            Register<UniversityProperty<HdjwError, HNUCourse, HNUCourseSelection, HNUCourseCategory, HdjwAuthResult>,
                HNUCourse, HdjwError, HNUCourseSelection>("HNU",
                new(HNUAuthChain.HdjwAuth, result => new HdjwClient(result), HNUCourse.BuildPresentedData(),
                    HNUCourseSelection.BuildPresentedData(), HNUCourse.Codec, HNUCourseSelection.Codec));

    public static readonly
        UniversityProperty<HdjwError, HNUCourse, HNUCourseSelection, HNUCourseCategory, HdjwAuthResult>
        DEBUG =
            Register<UniversityProperty<HdjwError, HNUCourse, HNUCourseSelection, HNUCourseCategory, HdjwAuthResult>,
                HNUCourse, HdjwError, HNUCourseSelection>("Debug",
                new(HNUAuthChain.DebugAuth, result => new HdjwDebugClient(result), HNUCourse.BuildPresentedData(),
                HNUCourseSelection.BuildPresentedData(), HNUCourse.Codec, HNUCourseSelection.Codec));
    private static TUniversity
        Register<TUniversity, TCourse, TError, TCourseSelection>(string name, TUniversity university)
        where TCourse : ICourse
        where TUniversity : IUniversity<TCourse, TError, TCourseSelection>, IMainWindowModelProvider
        where TCourseSelection : ICourseSelection
        where TError : BasicError
    {
        UniversityRegistry[name] = university;
        return university;
    }

    public static Result<Func<IMainWindowModel>, BasicError> LoginAndGetMainWindowModelFromId(string id, UsernamePassword profile)
    {
        return UniversityRegistry[id].LoginAndCreateMainWindowModel(profile);
    }

    internal static List<UniversityInfo> GetAllUniversities()
    {
        return UniversityRegistry.Keys.Select(id =>
        {
            var translationKey = $"university.{id}";
            return new UniversityInfo(Language.ResourceManager.GetString(translationKey, CultureInfo.CurrentCulture) ??
                                      throw new MissingMemberException($"Cannot find {translationKey} in language"), 
                id);
        }).ToList();
    }
}

internal class UniversityInfo
{
    public UniversityInfo(string displayName, string id)
    {
        DisplayName = displayName;
        Id = id;
    }

    internal string DisplayName { get; }
    internal string Id { get; }

    public override string ToString()
    {
        return DisplayName;
    }
}

public class UniversityProperty<TError, TCourse,TCourseSelection, TCourseCategory, TAuthResult> : IUniversity<TCourse, TError, TCourseSelection>, IMainWindowModelProvider
    where TError : BasicError
    where TCourse : Course<TCourse>
    where TAuthResult : AuthResult
    where TCourseCategory : ICourseCategory
    where TCourseSelection : class, ICourseSelection
{
    public UniversityProperty(AuthChain<TAuthResult> authChain,
        Func<TAuthResult, ISessionClient<TError, TCourseSelection, TCourse, TCourseCategory>> clientFunc,
        List<PresentedData<TCourse>> coursePresentedData, List<PresentedData<TCourseSelection>> selectionPresentedData,
        Codec<TCourse, TError> courseCodec, Codec<TCourseSelection, TError> selectionCodec)
    {
        AuthChain = authChain;
        ClientFunc = clientFunc;
        CoursePresentedData = coursePresentedData;
        SelectionPresentedData = selectionPresentedData;
        CourseCodec = courseCodec;
        SelectionCodec = selectionCodec;
    }

    public AuthChain<TAuthResult> AuthChain { get; }
    public Func<TAuthResult, ISessionClient<TError, TCourseSelection, TCourse, TCourseCategory>> ClientFunc { get; }
    public List<PresentedData<TCourse>> CoursePresentedData { get; }
    public List<PresentedData<TCourseSelection>> SelectionPresentedData { get; }
    public Codec<TCourse, TError> CourseCodec { get; }
    public Codec<TCourseSelection, TError> SelectionCodec { get; }

    public Result<Func<IMainWindowModel>, BasicError> LoginAndCreateMainWindowModel(UsernamePassword profile)
    {
        return AuthChain.Auth(profile, new WebClient()).Bind<Func<IMainWindowModel>>(result => new Func<IMainWindowModel>(() =>
            new MainWindowModel<UniversityProperty<TError, TCourse, TCourseSelection, TCourseCategory, TAuthResult>,
                TCourse, TError, TCourseCategory, TCourseSelection>(this, ClientFunc(result)))).CastError<BasicError>();
    }
}