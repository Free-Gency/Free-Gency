namespace FreeGency.Application.Common.Errors;

public record AppError(string code, string message, int statusCode,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IDictionary<string, string[]>? validationErrors = null)
{
    public static readonly AppError None =
        new(string.Empty, string.Empty, StatusCodes.Status200OK);

    #region GENERIC ERRORs
    public static AppError NotFound(string entity, object key) =>
        new($"{entity}.NotFound", $"{entity} with key '{key}' was not found.", StatusCodes.Status404NotFound);

    public static AppError NotActive(string entity, string name) =>
        new($"{entity}.NotActive", $"The {entity} '{name}' is currently inactive. Contact your administrator.", StatusCodes.Status403Forbidden);

    public static AppError Validation(string message, IDictionary<string, string[]>? errors = null) =>
        new("Validation.Failed", message, StatusCodes.Status400BadRequest, errors);

    public static AppError InsufficientWalletBalance(decimal needed, decimal available) =>
        new(
            "Wallet.InsufficientBalance",
            $"Insufficient wallet balance to fund this milestone. Needed ${needed:0.##}, available ${available:0.##}.",
            StatusCodes.Status400BadRequest);

    public static AppError Conflict(string message) =>
        new("Conflict", message, StatusCodes.Status409Conflict);

    public static AppError CrossTenantViolation() =>
        new("Tenant.CrossTenantViolation", "You do not have access to resources belonging to another institution.", StatusCodes.Status403Forbidden);

    public static AppError TenantNotResolved() =>
        new("Tenant.NotResolved", "The institution could not be determined from the current request.", StatusCodes.Status400BadRequest);

    public static AppError Failure(Exception? ex = null) =>
        new("Server.InvalidState", $"Details: {ex?.Message ?? "Internal API contract state configuration violation."}", StatusCodes.Status500InternalServerError);
    #endregion


    #region AUTH ERRORs
    public static AppError Unauthorized(string message = "Unauthorized.") =>
        new("Auth.Unauthorized", message, StatusCodes.Status401Unauthorized);

    public static AppError Forbidden(string message = "Forbidden.") =>
        new("Auth.Forbidden", message, StatusCodes.Status403Forbidden);

    public static AppError EmailNotConfirmed(string email) =>
        new("Auth.EmailNotConfirmed", $"The email address '{email}' has not been confirmed yet. Please check your inbox.", StatusCodes.Status403Forbidden);

    public static AppError InvalidCredentials() =>
        new("Auth.InvalidCredentials", "Invalid email address or password.", StatusCodes.Status401Unauthorized);
    public static AppError EmailAlreadyRegistered(string email) =>
        new("Auth.EmailAlreadyRegistered", $"The email address '{email}' is already registered.", StatusCodes.Status409Conflict);

    public static AppError AccountDeleted() =>
        new("Auth.AccountDeleted", "This account has been deleted and is no longer accessible.", StatusCodes.Status403Forbidden);

    public static AppError AccountBanned() =>
        new("Auth.AccountBanned", "This account has been suspended. Please contact support.", StatusCodes.Status403Forbidden);

    public static AppError RefreshTokenNotFound() =>
        new("Auth.RefreshTokenNotFound", "The provided refresh token does not exist.", StatusCodes.Status404NotFound);

    public static AppError RefreshTokenExpired() =>
        new("Auth.RefreshTokenExpired", "The refresh token has expired. Please log in again.", StatusCodes.Status401Unauthorized);

    public static AppError RefreshTokenRevoked() =>
        new("Auth.RefreshTokenRevoked", "The refresh token has already been revoked. Please log in again.", StatusCodes.Status401Unauthorized);

    public static AppError AccessTokenInvalid() =>
        new("Auth.AccessTokenInvalid", "The provided access token is invalid or malformed.", StatusCodes.Status401Unauthorized);
    #endregion


    #region CATEGORIES ERRORs
    public static AppError CategoryNameAlreadyExists(string name) =>
        new("Category.NameAlreadyExists",
            $"A category with the name '{name}' already exists.",
            StatusCodes.Status409Conflict);

    public static AppError SpecialtyDoesNotBelongToCategory(Guid specialtyId, Guid categoryId) =>
        new("Specialty.DoesNotBelongToCategory",
            $"Specialty '{specialtyId}' does not belong to category '{categoryId}'.",
            StatusCodes.Status400BadRequest);

    public static AppError SkillDoesNotBelongToSpecialty(Guid skillId) =>
        new("Skill.DoesNotBelongToSpecialty",
            $"Skill '{skillId}' does not belong to any Specialty.",
            StatusCodes.Status400BadRequest);

    public static AppError SubcategoryNameAlreadyExists(string name, string categoryName) =>
        new("Subcategory.NameAlreadyExists",
            $"A subcategory named '{name}' already exists under '{categoryName}'.",
            StatusCodes.Status409Conflict);
    #endregion


    #region CONVERSATIONS (CHAT) ERRORs
    public static AppError ConversationNotUnlocked(Guid itemId) =>
        new("Conversation.NotUnlocked",
            $"The chat for item '{itemId}' is not available yet. It unlocks only after a claim is approved.",
            StatusCodes.Status403Forbidden);

    public static AppError ConversationAlreadyExists(Guid itemId) =>
        new("Conversation.AlreadyExists",
            $"A conversation already exists between you and the other party for item '{itemId}'.",
            StatusCodes.Status409Conflict);

    public static AppError CannotDeleteMessage() =>
        new("Message.CannotDelete",
            "Messages cannot be deleted from FoundIt conversations.",
            StatusCodes.Status403Forbidden);
    #endregion


    #region NOTIFICATION ERRORs
    public static AppError NotificationDoesNotBelongToProfile(Guid notificationId) =>
        new("Notification.DoesNotBelongToProfile",
            $"Notification '{notificationId}' does not belong to your active profile.",
            StatusCodes.Status403Forbidden);
    #endregion


    #region FILE UPLOADS ERRORs
    public static AppError FileTooLarge(string fileName, long maxSizeInMb) =>
        new("File.TooLarge",
            $"File '{fileName}' exceeds the maximum allowed size of {maxSizeInMb}MB.",
            StatusCodes.Status400BadRequest);

    public static AppError InvalidFileType(string fileName, IEnumerable<string> allowed) =>
        new("File.InvalidType",
            $"File '{fileName}' has an unsupported type. Allowed types: {string.Join(", ", allowed)}.",
            StatusCodes.Status400BadRequest);

    public static AppError FileUploadFailed(string fileName) =>
        new("File.UploadFailed",
            $"Failed to upload file '{fileName}'. Please try again.",
            StatusCodes.Status500InternalServerError);

    public static AppError FileNotFound(string fileId) =>
        new("File.NotFound",
            $"File '{fileId}' was not found in storage.",
            StatusCodes.Status404NotFound);

    public static AppError FileDeletionFailed(string fileId) =>
        new("File.DeletionFailed",
            $"Failed to delete file '{fileId}' from storage.",
            StatusCodes.Status500InternalServerError);

    public static AppError NoFileProvided() =>
        new("File.NoFileProvided",
            "No file was provided. Please attach a file and try again.",
            StatusCodes.Status400BadRequest);
    #endregion


    #region SPECIALTIES ERRORs
    public static AppError SpecialtyNameAlreadyExists(string name) =>
        new("Specialty.NameAlreadyExists",
            $"A specialty with the name '{name}' already exists.",
            StatusCodes.Status409Conflict);

    public static AppError SpecialtyInvalidCategory(Guid categoryId) =>
        new("Specialty.InvalidCategory",
            $"Category '{categoryId}' was not found.",
            StatusCodes.Status404NotFound);
    #endregion


    #region SKILLS ERRORs
    public static AppError SkillNameAlreadyExists(string name) =>
        new("Skill.NameAlreadyExists",
            $"A skill with the name '{name}' already exists.",
            StatusCodes.Status409Conflict);
    #endregion

    #region TEAM JOBS ERRORs
    public static AppError TeamJobAlreadyClosed(Guid jobId) =>
        new("TeamJob.AlreadyClosed",
            $"Team job '{jobId}' is already closed.",
            StatusCodes.Status409Conflict);

    public static AppError TeamJobInvalidTeam(Guid teamId) =>
        new("TeamJob.InvalidTeam",
            $"Team '{teamId}' was not found.",
            StatusCodes.Status404NotFound);
    #endregion


    #region PLANS ERRORs
    public static AppError PlanLimitReached(string featureName, int? limit) =>
        new("Plan.LimitReached",
            $"You have reached your {featureName} limit ({(limit.HasValue ? limit.Value.ToString() : "unlimited")}) for this period. Upgrade your plan to continue.",
            StatusCodes.Status403Forbidden);

    public static AppError PlanFeatureNotIncluded(string featureName) =>
        new("Plan.NotIncluded",
            $"'{featureName}' is not included in your current plan. Upgrade to unlock it.",
            StatusCodes.Status403Forbidden);

    public static AppError PlanNotActive() =>
        new("Plan.NotActive",
            "Your subscription is not active. Please renew to continue.",
            StatusCodes.Status403Forbidden);
    #endregion

}
