using System.Text.Json.Nodes;
using CourseRush.Auth;
using CourseRush.Auth.HNU;
using CourseRush.Core.Network;
using CourseRush.HNU;

namespace CourseRush.CLI;

public class Program
{
    public static void Main(string[] args)
    {
        // var hdjwAuthResult = HNUAuthChain.HdjwAuth.Auth(
        // new UsernamePassword(
        // Environment.GetEnvironmentVariable("HNU_USERNAME") ??
        // throw new InvalidOperationException("Cannot get hnu user name from env var"),
        // Environment.GetEnvironmentVariable("HNU_PASSWORD") ??
        // throw new InvalidOperationException("Cannot get hnu user password from env var")), new WebClient());
        // hdjwAuthResult.Tee(result =>
        // {
        //     Console.WriteLine(result);
        //     var hdjwClient = new HdjwClient(result);
        //     hdjwClient.GetOngoingCourseSelections().Tee(ongoingCourseSelections =>
        //     {
        //         Console.WriteLine(string.Join(", ",ongoingCourseSelections));
        //         hdjwClient.GetSelectionClient(ongoingCourseSelections.First()).GetCategoriesInRound().Tee(list =>
        //         {
        //             foreach (var courseCategory in list)
        //             {
        //                 Console.WriteLine(courseCategory);
        //             }
        //         });
        //     });
        // });


        var fileStream = File.Open("H:\\CSharpeProjects\\CourseRush\\hnu_courses_type3.json", FileMode.Open);
        var array =
            JsonNode.Parse(fileStream)?["data"]?["showKclist"]?.AsArray();
        if (array is null) return;
        foreach (var jsonNode in array)
        {
            if (jsonNode != null)
            {
                HNUCourse.FromJson(jsonNode.AsObject()).Tee(Console.WriteLine);
                Console.WriteLine();
            }
        }
        fileStream.Close();
    }
}