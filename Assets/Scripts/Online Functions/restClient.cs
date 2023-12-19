using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class User
{
    public int id;
    public string username;
    public string password;
    public string firstname;
    public string lastname;
    public string created;
    public string lastseen;
    public int banned;
    public int isadmin;
   

    public static User CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<User>(jsonString);
    }

}

[System.Serializable]
public class Thread
{
    public int id;
    public string title;
    public int author;
    public string created;
    public int hidden;

    public static Thread CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<Thread>(jsonString);
    }
}

[System.Serializable]
public class Message
{
    public int id;
    public int thread;
    public string content;
    public string title;
    public int author;
    public int replyto;
    public string created;
    public string modified;
    public int hidden;

    public static Message CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<Message>(jsonString);
    }
}

[System.Serializable]
public class Score
{
    public int id;
    public int userid;
    public int currentscore;
    public int locationX;
    public int locationY;
    public int hidden;

    public static Score CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<Score>(jsonString);
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}

public class restClient : MonoBehaviour
{
    private string baseurl = "http://localhost/bb/api";
    public static bool loggedIn { get; private set; } = false;
    public static bool offlinePlay { get; private set; } = false;
    private bool objsInited = false;

    [SerializeField] private GameObject loginScreen;
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;

    [SerializeField] private TextMeshProUGUI userTag;

    [SerializeField] private GridLogic gridLogic;

    // User information.
    public User[] currUser;
    public Score[] userScore;

    // Database information.
    public User[] users; // array representing copy of the database (as the backend server provides)

    private float _timer;
    private float scoreSaveInterval = 2;

    private void Update()
    {
        // As long as we are logged in and have created a userScore we try to save the score on an interval.
        if (loggedIn && userScore != null)
        {
            _timer += Time.deltaTime;
            if(_timer > scoreSaveInterval)
            {
                userScore[0].locationX = gridLogic.PlayerPosX;
                userScore[0].locationY = gridLogic.PlayerPosY;
                userScore[0].currentscore = GridLogic.PlayerScore;
                _timer = 0;
                StartCoroutine(SaveScore());
                Debug.Log("I tried to save the score.");
            }
        }
    }

    private string fixJson(string value)
    {
        value = "{\"Items\":" + value + "}";
        return value;
    }

    public void LoginHelper()
    {
        StartCoroutine(Login(usernameField.text, passwordField.text));
        //StartCoroutine(PollThreads());
    }

    public void PlayOffline()
    {
        offlinePlay = true;
        loginScreen.SetActive(false);
        userTag.text = "You are playing offline.\nYour score is not being tracked.";
    }

    // perform one-time login at the beginning of a connection
    IEnumerator Login(string username, string password)
    {
       
        UnityWebRequest www = UnityWebRequest.Post(baseurl + "/login", "{ \"username\": \"" + username + "\", \"password\": \"" + password + "\" }", "application/json");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            var text = www.downloadHandler.text;
            Debug.Log(baseurl+ "/login");
            Debug.Log(www.error);
            Debug.Log(text);
        }
        else
        {
            Debug.Log("Login complete!");
            loggedIn = true;
            loginScreen.SetActive(false);
            var text = www.downloadHandler.text;
            string jsonString = fixJson(text); // add Items: in front of the json array
            currUser = JsonHelper.FromJson<User>(jsonString);
            Debug.Log(text);
            // Try to get user's score now that they are logged in.
            StartCoroutine(GetUserScore());

            userTag.text = "Hello " + currUser[0].firstname + " " + currUser[0].lastname + "!"
                + "\nYou're logged in as " + currUser[0].username;
        }
    }

    IEnumerator GetUserScore()
    {
        while (!loggedIn) yield return new WaitForSeconds(5);
        
        UnityWebRequest www = UnityWebRequest.Get(baseurl + "/scores/" + currUser[0].id);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            var text = www.downloadHandler.text;
            Debug.Log(baseurl + "/scores/" + currUser[0].id);
            Debug.Log(www.error);
            Debug.Log(text);
            // If we found no user score, we create one. This is very insecure and blaa blaa, but it's how this is supposed to work now.
            StartCoroutine(CreateUserScore());
        }
        else
        {
            var text = www.downloadHandler.text;
            Debug.Log("score download complete: " + text);
            loggedIn = true;
            string jsonString = fixJson(text);
            userScore = JsonHelper.FromJson<Score>(jsonString);
            GridLogic.PlayerScore = userScore[0].currentscore;
            gridLogic.PlayerPosX = userScore[0].locationX;
            gridLogic.PlayerPosY = userScore[0].locationY;
        }
    }

    IEnumerator CreateUserScore()
    {
        while (!loggedIn) yield return new WaitForSeconds(5);
        //userScore[0] = new Score();
        //userScore[0].userid = currUser[0].id;
        Debug.Log("Trying to create new user score.");

        UnityWebRequest www = UnityWebRequest.Post(baseurl + "/scores/" + currUser[0].id, JsonHelper.ToJson<Score>(userScore), "application/json");
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            var text = www.downloadHandler.text;
            Debug.Log(baseurl + "/scores");
            Debug.Log(www.error);
            Debug.Log(text);
        }
        else
        {
            var text = www.downloadHandler.text;
            Debug.Log("New score created: " + text);
            loggedIn = true;
            // Now that we created the score, we need to get it. This is a very dangerous way to make this, because it loops, but there's kind of too much
            // to do already, so I'm not sweating the details.
            StartCoroutine(GetUserScore());
        }
    }

    IEnumerator SaveScore()
    {
        while (!loggedIn) yield return new WaitForSeconds(2);
        UnityWebRequest www = UnityWebRequest.Put(baseurl + "/scores/" + currUser[0].id, JsonHelper.ToJson<Score>(userScore));
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            var text = www.downloadHandler.text;
            Debug.Log(baseurl + "/scores/" + currUser[0].id);
            Debug.Log(www.error);
            Debug.Log(text);
        }
        else
        {
            var text = www.downloadHandler.text;
            // This doesn't actually check if the score was updated, but rather if we found the score we wanted to update.
            Debug.Log("Updated score was found: " + text);
            loggedIn = true;
        }
    }

    // perform asynchronnous polling of threads information every X seconds after login succesful
    IEnumerator PollThreads()
    {
        while(!loggedIn) yield return new WaitForSeconds(10); // wait for login to happen

        while (true){
            UnityWebRequest www = UnityWebRequest.Get(baseurl + "/threads");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                var text = www.downloadHandler.text;
                Debug.Log(baseurl+ "/threads");
                Debug.Log(www.error);
                Debug.Log(text);
            }
            else
            {
                var text = www.downloadHandler.text;
                Debug.Log("threads download complete: "+text);
                loggedIn = true;
                // TODO: handle messages JSON somehow
            }
            yield return new WaitForSeconds(5); 
        }
        
    }

    // I'm not using this for anything.
    // perform asynchronnous polling of users information every X seconds after login succesfull
    IEnumerator PollUsers()
    {
        
        while(!loggedIn) yield return new WaitForSeconds(10); // wait for login to happen

        while (true){
            UnityWebRequest www = UnityWebRequest.Get(baseurl + "/users");
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                var text = www.downloadHandler.text;
                Debug.Log(baseurl+ "/users");
                Debug.Log(www.error);
                Debug.Log(text);
            }
            else
            {
                var text = www.downloadHandler.text;
                Debug.Log("users download complete: "+text);
                loggedIn = true;
                string jsonString = fixJson(text); // add Items: in front of the json array
                users = JsonHelper.FromJson<User>(jsonString); // convert json to User-array (public users) // overwrite data each update!
                // SEE :
                // https://stackoverflow.com/questions/36239705/serialize-and-deserialize-json-and-json-array-in-unity/36244111#36244111

                if(!objsInited){
                    GameObject userSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Vector3 position = new Vector3(2.0f,2.0f,0.0f);
                    float gap = 2;
                    for (int i = 0; i < users.Length; i++)
                    {
                        GameObject newObject = (GameObject)Instantiate(userSphere, position, Quaternion.identity);
                        position.x += gap;
                        newObject.name = users[i].username;
                        
                    }
                    objsInited=true;
                }else{
                        // TODO: only update users, e.g. add new user or update changed properties of existing one, need to compare existing ones
                }
            }
            yield return new WaitForSeconds(60); // users may not update very often
        }
        
    }
}
