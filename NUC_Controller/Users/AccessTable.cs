using System.Collections.Generic;

namespace NUC_Controller.Users
{
    public static class AccessTable
    {
        public static Dictionary<ActionType, List<UserType>> accessChecker;

        static AccessTable()
        {
            accessChecker = new Dictionary<ActionType, List<UserType>>();
            //Read
            accessChecker.Add(ActionType.readGlobals, new List<UserType>() { UserType.Admin, UserType.Extended });
            accessChecker.Add(ActionType.ReadEvents, new List<UserType>() { UserType.Admin, UserType.Extended, UserType.Simple });
            accessChecker.Add(ActionType.ReadFaces, new List<UserType>() { UserType.Admin, UserType.Extended, UserType.Simple });
            accessChecker.Add(ActionType.ReadBodies, new List<UserType>() { UserType.Admin, UserType.Extended, UserType.Simple });
            accessChecker.Add(ActionType.ReadConfig, new List<UserType>() { UserType.Admin, UserType.Extended });
            accessChecker.Add(ActionType.ReadUsers, new List<UserType>() { UserType.Admin, UserType.Extended });

            //Change
            accessChecker.Add(ActionType.CreateUser, new List<UserType>() { UserType.Admin });
            accessChecker.Add(ActionType.RemoveUser, new List<UserType>() { UserType.Admin });
            accessChecker.Add(ActionType.ChangeExistingUser, new List<UserType>() { UserType.Admin, UserType.Extended });
            accessChecker.Add(ActionType.ChangeConfig, new List<UserType>() { UserType.Admin, UserType.Extended });

            //Special
            accessChecker.Add(ActionType.ExportEventsToFile, new List<UserType>() { UserType.Admin, UserType.Extended, UserType.Simple });
            accessChecker.Add(ActionType.PerformCalibration, new List<UserType>() { UserType.Admin, UserType.Extended });

        }
    }
}
