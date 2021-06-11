using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Helpers
{
    public class Consts
    {
        //Status
        public static int STATUS_ACTIVE = 1;
        public static int STATUS_INACTIVE = 0;
        //Test
        public static int TEST_HT_TYPE_ID = 2;
        public static int TEST_THPTQG_TYPE_ID = 1;
        //Role
        public static string ADMIN_ROLE = "Admin";
        public static string USER_ROLE = "User";
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
        //Transcript
        public static int TRANSCRIPT_TYPE_HB = 2;
        public static int TRANSCRIPT_TYPE_THPTQG = 1;
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
    }

    public class LogEvent
    {
        //Subject
        public const int GetAllSubjects = 1000;
        public const int Login = 2000;
    }
}
