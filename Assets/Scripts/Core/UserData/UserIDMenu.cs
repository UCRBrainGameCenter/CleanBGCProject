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
        //Should pool these instead... Oh well
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
            newUserButton.userButton.onClick.AddListener(() => LogInWithUser(userNameCopy));
            newUserButton.deleteButton.onClick.AddListener(() => RequestDeleteUser(userNameCopy));

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
            mode: ModalDialog.Mode.InputConfirmCancel,
            headerText: "Creating New User",
            bodyText: "New User Name:",
            inputCallback: UserNameSubmitted);
    }

    private void UserNameSubmitted(
        ModalDialog.Response response,
        string userName)
    {
        switch (response)
        {
            case ModalDialog.Response.Confirm:
                Submit(userName);
                break;

            case ModalDialog.Response.Cancel:
                break;

            case ModalDialog.Response.Accept:
            default:
                Debug.LogError($"Unexpected ModalDialog.Response: {response}");
                break;
        }
    }

    private void Submit(string newName)
    {
        newName = newName.Trim();

        //Abort and warn if the name is bad or not unique
        if (!TestNameValid(newName))
        {
            return;
        }

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

        //Switch to main menu
        menuManager.SetWindowState(MenuManager.WindowState.Title);
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
    public void LogInWithUser(string userName)
    {
        if (LoadUser(userName))
        {
            menuManager.SetWindowState(MenuManager.WindowState.Title);
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

    /// <summary>
    /// Spawn user delete confirmation
    /// </summary>
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
}
