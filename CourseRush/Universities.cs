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
using CourseRush.Models;
using Resultful;

namespace CourseRush;

public interface IMainWindowModelProvider
{
    public Result<Func<IMainWindowModel>, BasicError> LoginAndCreateMainWindowModel(UsernamePassword usernamePassword);
}

public interface IUniversity<TCourse, TError, TCourseSelection> 
    where TCourse : ICourse, IPresentedDataProvider<TCourse>, IJsonSerializable<TCourse, TError> 
    where TCourseSelection : ICourseSelection, IPresentedDataProvider<TCourseSelection>, IJsonSerializable<TCourseSelection, TError>
    where TError : BasicError, ICombinableError<TError>;

[SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeNotEvident")]
public static class Universities
{
    private static readonly Dictionary<string, IMainWindowModelProvider> UniversityRegistry = new();

    public static readonly
        UniversityProperty<HdjwError, HNUCourse, HNUSelectedCourse, HNUCourseSelection, HNUCourseCategory, HdjwAuthResult, HdjwClient>
        HNU = Register<HdjwError, HNUCourse, HNUSelectedCourse, HNUCourseSelection, HNUCourseCategory, HdjwAuthResult, HdjwClient>("HNU", HNUAuthChain.HdjwAuth);

    public static readonly
        UniversityProperty<HdjwError, HNUCourse, HNUSelectedCourse, HNUCourseSelection, HNUCourseCategory, HdjwAuthResult, HdjwDebugClient>
        DEBUG = Register<HdjwError, HNUCourse, HNUSelectedCourse, HNUCourseSelection, HNUCourseCategory, HdjwAuthResult, HdjwDebugClient>("Debug", HNUAuthChain.DebugAuth);
    
    private static UniversityProperty<TError, TCourse, TSelectedCourse, TCourseSelection, TCourseCategory, TAuthResult, TSelectionClient>
        Register<TError, TCourse, TSelectedCourse, TCourseSelection, TCourseCategory, TAuthResult, TSelectionClient>(string name, AuthChain<TAuthResult> authChain)
        where TCourse : Course<TCourse>, IPresentedDataProvider<TCourse>, IJsonSerializable<TCourse, TError>
        where TCourseSelection : class, ICourseSelection, IPresentedDataProvider<TCourseSelection>, IJsonSerializable<TCourseSelection, TError>
        where TError : BasicError, ICombinableError<TError>, ISelectionError
        where TAuthResult : AuthResult
        where TCourseCategory : ICourseCategory
        where TSelectionClient : ISessionClient<TError, TCourseSelection, TCourse, TSelectedCourse, TCourseCategory>, IResultConvertible<TAuthResult, TSelectionClient>
        where TSelectedCourse : TCourse, ISelectedCourse, IPresentedDataProvider<TSelectedCourse>
    {
        var universityProperty = new UniversityProperty<TError, TCourse, TSelectedCourse, TCourseSelection, TCourseCategory, TAuthResult, TSelectionClient>(authChain);
        UniversityRegistry[name] = universityProperty;
        return universityProperty;
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

internal class UniversityInfo(string displayName, string id)
{
    internal string DisplayName { get; } = displayName;
    internal string Id { get; } = id;

    public override string ToString()
    {
        return DisplayName;
    }
}

public class UniversityProperty<TError, TCourse, TSelectedCourse, TCourseSelection, TCourseCategory, TAuthResult, TSelectionClient>
    (AuthChain<TAuthResult> authChain)

    : IUniversity<TCourse, TError, TCourseSelection>, IMainWindowModelProvider
    where TError : BasicError, ICombinableError<TError>, ISelectionError
    where TCourse : Course<TCourse>, IPresentedDataProvider<TCourse>, IJsonSerializable<TCourse, TError>
    where TAuthResult : AuthResult
    where TCourseCategory : ICourseCategory
    where TCourseSelection : class, ICourseSelection, IPresentedDataProvider<TCourseSelection>, IJsonSerializable<TCourseSelection, TError>
    where TSelectionClient : ISessionClient<TError, TCourseSelection, TCourse, TSelectedCourse, TCourseCategory>, IResultConvertible<TAuthResult, TSelectionClient>
    where TSelectedCourse : TCourse, ISelectedCourse, IPresentedDataProvider<TSelectedCourse>
{
    public Result<Func<IMainWindowModel>, BasicError> LoginAndCreateMainWindowModel(UsernamePassword profile)
    {
        return authChain.Auth(profile, new WebClient()).Bind<Func<IMainWindowModel>>(result => new Func<IMainWindowModel>(() =>
            new MainWindowModel<UniversityProperty<TError, TCourse, TSelectedCourse, TCourseSelection, TCourseCategory, TAuthResult, TSelectionClient>,
                TCourse, TSelectedCourse, TError, TCourseCategory, TCourseSelection>(this, TSelectionClient.CreateFromResult(result)))).CastError<BasicError>();
    }
}