namespace NUC_Controller.Users
{
    public enum ActionType
    {
        readGlobals,
        ReadEvents,
        ReadFaces,
        ReadBodies,
        ReadConfig,
        ReadUsers,

        ExportEventsToFile,
        PerformCalibration,

        changeGlobals,
        ChangeConfig,
        CreateUser,
        RemoveUser,
        ChangeExistingUser
    }
}
