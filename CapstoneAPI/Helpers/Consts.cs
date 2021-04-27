using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CapstoneAPI.Helpers
{
    public class Consts
    {
        public static int STATUS_ACTIVE = 1;
        public static int STATUS_INACTIVE = 0;
        public static int TEST_HT_TYPE_ID = 2;
        public static int TEST_THPTQG_TYPE_ID = 1;
        public static int DEFAULT_WEIGHT_NUMBER = 1;
        public static int NUMBER_OF_SUGGESTED_GROUP = 3;
        public static int NUMBER_OF_SUGGESTED_MAJOR = 5;
        public static int DEFAULT_MAX_SCORE = 10;
        public static string ADMIN_ROLE = "Admin";
        public static string USER_ROLE = "User";
        public static string FIREBASE_KEY_PATH = "FirebaseKey\\unilinks-41d0e-firebase-adminsdk-th8o0-c0b4d125e8.json";
        public static int YEAR_2019 = 2019;
        public static int YEAR_2020 = 2020;
        public static int NEAREST_YEAR = 2020;
        public static int TOKEN_EXPIRED_TIME = 60 * 60;
        public static int REQUIRED_NUMBER_SUBJECTS = 3;

        //Fibase for Logo University

        public static List<string> IMAGE_EXTENSIONS = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".JFIF" };
        public static string LOGO_FOLDER = "abc/AvatarUniversity";
        public static string API_KEY = "AIzaSyBHrI1bDdG56ELUdBh05f3yOkNliAy8GUY";
        public static string BUCKET = "unilinks-41d0e.appspot.com";
        public static string AUTH_MAIL = "storage@gmail.com";
        public static string AUTH_PASSWORD = "Matkhau123";

    }                                             
}
