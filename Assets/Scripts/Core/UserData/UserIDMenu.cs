using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BGC.MonoUtility;
using BGC.Users;
using BGC.UI.Dialogs;
using BGC.UI.Panels;

public class UserIDMenu : ModePanel
{
    [Header("Engine References")]
    [SerializeField]
    private MenuManager menuManager = null;

    [Header("UserID Menu Elements")]
    [SerializeField]
    private GameObject userButtonPanel = null;
    [SerializeField]
    private Button createNewUserButton = null;
    [SerializeField]
    private StatusPanel statusPanel = null;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject userButtonTemplate = null;

    private Dictionary<string, UserButton> userButtons = new Dictionary<string, UserButton>();

    void Awake()
    {
        createNewUserButton.onClick.AddListener(CreateUserButtonPressed);
    }

    #region ModePanel

    public override void FocusAcquired()
    {
        foreach (UserButton userButton in userButtons.Values)
        {
            Destroy(userButton.gameObject);
        }

        userButtons.Clear();

        //Load the data into memory and create buttons
        foreach (string userName in PlayerData.GetUserNames())
        {
            string userNameCopy = userName;
            GameObject newButton = Instantiate(userButtonTemplate, userButtonPanel.transform, false);


            UserButton newUserButton = newButton.GetComponent<UserButton>();
            newUserButton.userText.text = userNameCopy;
            newUserButton.userButton.onClick.AddListener(() =>
            {
                LogInWithUser(userNameCopy);
            });

            newUserButton.deleteButton.onClick.AddListener(() =>
            {
                RequestDeleteUser(userNameCopy);
            });

            userButtons.Add(userNameCopy, newUserButton);
        }

        //Show the proper Title
        UpdateUI();
    }

    public override void FocusLost()
    {
        //Do Nothing
    }

    #endregion ModePanel

    public void LockStateChanged()
    {
        UpdateUI();
    }

    private void CreateUserButtonPressed()
    {
        ModalDialog.ShowInputModal(
            headerText: "Creating New User",
            primaryBodyText: "New User Name:",
            secondaryBodyText: "Server Code (Optional):",
            inputCallback: UserNameCodeSubmitted);
    }

    private void UserNameCodeSubmitted(
        ModalDialog.Response response,
        string userName,
        string serverCode)
    {
        switch (response)
        {
            case ModalDialog.Response.Confirm:
                Submit(userName, serverCode);
                break;

            case ModalDialog.Response.Cancel:
                break;

            case ModalDialog.Response.Accept:
            default:
                Debug.LogError($"Unexpected ModalDialog.Response: {response}");
                break;
        }
    }

    private void Submit(string newName, string code)
    {
        newName = newName.Trim();
        code = code.Trim();

        //Abort and warn if the name is bad or not unique
        if (!TestNameValid(newName))
        {
            return;
        }


        if (code != "")
        {
            //Server User Creation

            //Abort if the code is malformed
            if (!TestCodeValid(code))
            {
                return;
            }

            HandleServerNameCreation(
                code: code,
                newName: newName,
                callback: () => menuManager.SetWindowState(MenuManager.WindowState.Title));

        }
        else
        {
            //Local User Creation

            //Check if user addition was successful
            if (PlayerData.AddUser(newName) == false)
            {
                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Invalid User Name",
                    bodyText: "There was a problem creating user name.");
                return;
            }

            if (LoadUser(newName) == false)
            {
                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Error",
                    bodyText: $"Unable to log into user {newName}.");

                return;
            }

            SettingsMenu.PullPushableSettings();

            //if (SettingsMenu.GetSettingBool("StudyMode"))
            //{
            //    if (!PrepareProtocol(newName))
            //    {
            //        return;
            //    }
            //}

            //Switch to main menu
            menuManager.SetWindowState(MenuManager.WindowState.Title);
        }
    }

    private void UpdateUI()
    {
        bool bLocked = PlayerData.IsLocked;

        //Only show delete button if we're setting them active and the user exists
        foreach (UserButton userButton in userButtons.Values)
        {
            userButton.deleteButtonFrame.SetActive(!bLocked);
        }
    }

    /// <summary>
    /// Set current user and load the Menu scene.
    /// </summary>
    /// <param name="userName"></param>
    public void LogInWithUser(string userName)
    {
        if (LoadUser(userName))
        {
            //if (PrepareProtocol(userName))
            {
                menuManager.SetWindowState(MenuManager.WindowState.Title);
            }
        }
    }

    /// <summary>
    /// Set current user.  Returns success.
    /// </summary>
    private bool LoadUser(string userName)
    {
        bool loggedIn = PlayerData.LogIn(
            userName: userName,
            userChangingCallback: LogManager.ClearAllLogs);

        if (!loggedIn)
        {
            ModalDialog.ShowSimpleModal(
                mode: ModalDialog.Mode.Accept,
                headerText: "Load Failed",
                bodyText: $"Unable to load user {userName}");

            return false;
        }

        return true;
    }

    // Left as an example for Protocol Use
    //
    ///// <summary>
    ///// In StudyMode, load the indicated protocol, and check to see if the subject has completed the study.
    ///// </summary>
    ///// <param name="userName"></param>
    ///// <returns></returns>
    //public bool PrepareProtocol(string userName)
    //{
    //    ProtocolStatus status = ProtocolManager.TryUpdateProtocol(
    //        protocolName: SettingsMenu.GetSettingString(SettingsMenu.Keys.ProtocolSet),
    //        protocolID: SettingsMenu.GetSettingInt(SettingsMenu.Keys.ProtocolId),
    //        sessionIndex: ProtocolManager.SessionNumber,
    //        sessionElementIndex: ProtocolManager.ElementNumber);

    //    switch (status)
    //    {
    //        case ProtocolStatus.SessionReady:
    //            return true;

    //        case ProtocolStatus.SessionLimitExceeded:
    //            Debug.Log("SessionLimitExceeded");

    //            ModalDialog.ShowSimpleModal(
    //                mode: ModalDialog.Mode.ConfirmCancel,
    //                headerText: "Study Finished",
    //                bodyText: $"User {userName} has already completed the study.\n\nRestart?",
    //                callback: (resp) =>
    //                {
    //                    switch (resp)
    //                    {
    //                        case ModalDialog.Response.Confirm:
    //                            ProtocolManager.SessionNumber = 0;
    //                            ProtocolManager.ElementNumber = 0;
    //                            LogInWithUser(userName);
    //                            break;
    //                        case ModalDialog.Response.Cancel:
    //                            //Do Nothing
    //                            break;
    //                        default:
    //                            Debug.LogError($"Unexpected ModalDialog.Response: {resp}");
    //                            break;
    //                    }
    //                });
    //            return false;

    //        case ProtocolStatus.Uninitialized:
    //            Debug.Log("Uninitialized");

    //            ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
    //                headerText: "Protocol Failure",
    //                bodyText: $"Failed to load or locate protocol set " +
    //                    $"\"{SettingsMenu.GetSettingString(SettingsMenu.Keys.ProtocolSet)}\".  " +
    //                    $"Change to valid value, like \"DefaultSet\", before proceeding.");
    //            return true;

    //        case ProtocolStatus.InvalidProtocol:
    //            Debug.Log("InvalidProtocol");

    //            ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
    //                headerText: "Protocol Failure",
    //                bodyText: $"Invalid protocolID \"" +
    //                    $"{SettingsMenu.GetSettingInt(SettingsMenu.Keys.ProtocolId)}\".\n\n" +
    //                    $"Change to a valid value before proceeding.");
    //            return true;

    //        case ProtocolStatus.SessionElementLimitExceeded:
    //            Debug.Log("SessionElementLimitExceeded");

    //            //Start at the beginning of next session
    //            ++ProtocolManager.SessionNumber;
    //            ProtocolManager.ElementNumber = 0;

    //            //Retry loading
    //            return PrepareProtocol(userName);

    //        case ProtocolStatus.SessionFinished:
    //        default:
    //            Debug.LogError($"Unexpected ProtocolStatus: {status}");

    //            ModalDialog.ShowSimpleModal(
    //                mode: ModalDialog.Mode.Accept,
    //                headerText: "Load user error",
    //                bodyText: $"There was an error - unable to Initiate Study.  Error code: {status}");
    //            return true;
    //    }
    //}

    /// <summary>
    /// Spawn user delete confirmation
    /// </summary>
    /// <param name="userName"></param>
    public void RequestDeleteUser(string userName)
    {
        ModalDialog.ShowSimpleModal(ModalDialog.Mode.ConfirmCancel,
            headerText: "Confirm Delete",
            bodyText: $"Are you sure you want to delete user \"{userName}\"?",
            callback: (response) =>
            {
                switch (response)
                {
                    case ModalDialog.Response.Confirm:
                        DeleteUser(userName);
                        break;

                    case ModalDialog.Response.Cancel:
                        //Do Nothing
                        break;

                    default:
                        Debug.LogError($"Unrecognized ModalDialog.Response: {response}");
                        break;
                }
            });
    }

    /// <summary>
    /// Delete the specified user's data
    /// </summary>
    /// <param name="userName"></param>
    public void DeleteUser(string userName)
    {
        //Delete associated Data
        PlayerData.DeleteUserData(userName);

        //Clear associated Name
        if (userButtons.ContainsKey(userName))
        {
            Destroy(userButtons[userName].gameObject);
            userButtons.Remove(userName);
        }
        else
        {
            Debug.Log($"Failed to find button associate with userName: {userName}");
        }
    }

    private bool TestNameValid(string newName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                headerText: "Invalid User Name",
                bodyText: "You cannot add a user with an empty name.");
            return false;
        }

        if (newName.Contains("/") || newName.Contains(".") || newName.Contains("\\"))
        {
            ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                headerText: "Invalid User Name",
                bodyText: "Name is invalid. Cannot contain characters:'/', '.', or '\\'.");
            return false;
        }

        //Check if user name is available
        if (PlayerData.UserExists(newName))
        {
            ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                headerText: "Invalid User Name",
                bodyText: "User already exists.  Users must have unique names.");
            return false;
        }

        return true;
    }

    private bool TestCodeValid(string code)
    {
        if (code.Contains("/") || code.Contains(".") || code.Contains("\\") || code.Contains(":"))
        {
            ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                headerText: "Invalid Code",
                bodyText: "Server code is invalid. It cannot contain any special characters.\n" +
                    "If you have not been given a server code, leave it blank.");
            return false;
        }

        return true;
    }

    private void HandleServerNameCreation(
        string code,
        string newName,
        Action callback = null)
    {
        code = code.ToLowerInvariant();
        statusPanel.gameObject.SetActive(true);

        //Special server code goes here
    }


}
