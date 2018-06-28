namespace NUC_Controller.Users
{
    public enum ActionType
    {
        ReadEvents,
        ReadFaces,
        ReadBodies,
        ReadConfig,
        ReadUsers,

        ExportEventsToFile,
        PerformCalibration,

        ChangeConfig,
        CreateUser,
        RemoveUser,
        ChangeExistingUser
    }
}
