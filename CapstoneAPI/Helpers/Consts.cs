using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Helpers
{
    public class Consts
    {
        //Article
        public static string ARTICLE_FOLDER = "Article";
        public static int STATUS_PUBLISHED = 3;
        //Status
        public static int STATUS_ACTIVE = 1;
        public static int STATUS_INACTIVE = 0;
        //Test
        public static string QUESTION_FOLDER = "Question";
        public static int TEST_HT_TYPE_ID = 2;
        public static int TEST_THPTQG_TYPE_ID = 1;
        //Firebase
        public static string FIREBASE_KEY_PATH = "FirebaseKey\\unilinks-41d0e-firebase-adminsdk-th8o0-c0b4d125e8.json";
        //Year
        public static int YEAR_2019 = 2019;
        public static int YEAR_2020 = 2020;
        public static int NEAREST_YEAR = 2020;
        public static int CURRENT_YEAR = 2021;
        //Token
        public static int TOKEN_EXPIRED_TIME = 60 * 60 * 60;
        //Suggestion
        public static int REQUIRED_NUMBER_SUBJECTS = 3;
        public static int NUMBER_OF_SUGGESTED_GROUP = 3;
        public static int NUMBER_OF_SUGGESTED_MAJOR = 5;
        public static int DEFAULT_WEIGHT_NUMBER = 1;
        public static int DEFAULT_MAX_SCORE = 10;

        //Rank type
        public static int RANK_TYPE_THPTQG = 1;
        public static int RANK_TYPE_HT = 3;
        public static int RANK_TYPE_HB = 2;
        //Fibase for Logo University
        public static List<string> IMAGE_EXTENSIONS = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".JFIF" };
        public static string LOGO_FOLDER = "abc/AvatarUniversity";
        public static string API_KEY = "AIzaSyBHrI1bDdG56ELUdBh05f3yOkNliAy8GUY";
        public static string BUCKET = "unilinks-41d0e.appspot.com";
        public static string AUTH_MAIL = "storage@gmail.com";
        public static string AUTH_PASSWORD = "Matkhau123";

        public const int SECOND = 1;
        public const int MINUTE = 60 * SECOND;
        public const int HOUR = 60 * MINUTE;
        public const int DAY = 24 * HOUR;
        public const int MONTH = 30 * DAY;
    }

    public class LogEvent
    {
        //Subject
        public const int GetAllSubjects = 1000;
        public const int Login = 2000;
    }

    public static class Roles
    {
        public const string Admin = "3";
        public const string Staff = "1";
        public const string Student = "2";
    }

    //check status
    /*
     * -1: All
     * 0: New
     * 1: Approved
     * 2: Rejected
     * 3: Published
     * 4: Expired
     * 5: (Considered)
     */
    public static class Articles
    {
        public const int New = 0;
        public const int Approved = 1;
        public const int Rejected = 2;
        public const int Published = 3;
        public const int Expired = 4;
        public const int Considered = 5;
    }

    //1: New article
    //2: New rank
    //3: Update tt uni
    //6: Thay doi tt suggest => remove follow
    public static class NotificationTypes
    {
        public const int NewArticle = 1;
        public const int NewRank = 2;
        public const int UpdateUniversity = 3;
        public const int UpdateSuggestInfo = 6;
    }

    public static class HomeArticleTypes
    {
        public const int Hot = 1;
        public const int Today = 2;
        public const int Past = 3;
    }

    public static class AdmissionMethodTypes
    {
        public const int HocBa = 2;
        public const int THPTQG = 1;
    }

    public static class Subjects
    {
        public const int Literature = 10;
    }

    public static class QuestionTypes
    {
        public const int SingleChoice = 1;
        public const int MultipleChoice = 2;
    }
    public static class UniversityRatios
    {
        public const double Green = 1;
        public const double Yellow = 1.5;
        public const int GreenGroup = 1;
        public const int YellowGroup = 2;
        public const int RedGroup = 3;
    }

    public static class TranscriptTypes
    {
        public const int HocBa = 2;
        public const int THPTQG = 1;
        public const int ThiThu = 3;
    }

    public static class CronExporessionType
    {
        public const int EachHours = 1;
        public const int SpecificHour = 2;
    }


}
