using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Common.SeedWork
{
    public class PathWithoutToken
    {
        public const string COMMON_PATH = "commons";
        public const string USER_LOGIN = COMMON_PATH + "/sign-in";
        public const string USER_SIGNUP = COMMON_PATH + "/sign-up";
        public const string REFRESH_TOKEN = COMMON_PATH + "/refresh-token";
        public const string MANGA_PATH = "manga";
        public const string CHAPTER_PATH = "chapter";
        public const string GENRE_PATH = "genre";
        public const string COMMENT_PATH = "comment";
        public const string NOTIFICATION = "/notification";
    }
}
