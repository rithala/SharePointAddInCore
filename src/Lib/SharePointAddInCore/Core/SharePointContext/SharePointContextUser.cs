using Newtonsoft.Json;

namespace SharePointAddInCore.Core.SharePointContext
{
    public class SharePointContextUser
    {
        [JsonProperty("Id")]
        public long Id { get; }

        [JsonProperty("IsHiddenInUI")]
        public bool IsHiddenInUi { get; }

        [JsonProperty("LoginName")]
        public string LoginName { get; }

        [JsonProperty("Title")]
        public string Title { get; }

        [JsonProperty("PrincipalType")]
        public long PrincipalType { get; }

        [JsonProperty("Email")]
        public string Email { get; }

        [JsonProperty("Expiration")]
        public string Expiration { get; }

        [JsonProperty("IsEmailAuthenticationGuestUser")]
        public bool IsEmailAuthenticationGuestUser { get; }

        [JsonProperty("IsShareByEmailGuestUser")]
        public bool IsShareByEmailGuestUser { get; }

        [JsonProperty("IsSiteAdmin")]
        public bool IsSiteAdmin { get; }

        [JsonProperty("UserId")]
        public UserId UserId { get; }

        [JsonProperty("UserPrincipalName")]
        public string UserPrincipalName { get; }

        [JsonConstructor]
        internal SharePointContextUser(
            long id,
            bool isHiddenInUi,
            string loginName,
            string title,
            long principalType,
            string email,
            string expiration,
            bool isEmailAuthenticationGuestUser,
            bool isShareByEmailGuestUser,
            bool isSiteAdmin,
            UserId userId,
            string userPrincipalName)
        {
            Id = id;
            IsHiddenInUi = isHiddenInUi;
            LoginName = loginName;
            Title = title;
            PrincipalType = principalType;
            Email = email;
            Expiration = expiration;
            IsEmailAuthenticationGuestUser = isEmailAuthenticationGuestUser;
            IsShareByEmailGuestUser = isShareByEmailGuestUser;
            IsSiteAdmin = isSiteAdmin;
            UserId = userId;
            UserPrincipalName = userPrincipalName;
        }
    }

    public class UserId
    {
        [JsonProperty("NameId")]
        public string NameId { get; }

        [JsonProperty("NameIdIssuer")]
        public string NameIdIssuer { get; }

        [JsonConstructor]
        internal UserId(string nameId, string nameIdIssuer)
        {
            NameId = nameId;
            NameIdIssuer = nameIdIssuer;
        }
    }
}
