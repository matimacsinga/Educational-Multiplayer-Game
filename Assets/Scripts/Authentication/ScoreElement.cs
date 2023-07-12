using UnityEngine;
using TMPro;

public class ScoreElement : MonoBehaviour{

    public TMP_Text usernameText;
    public TMP_Text lessonsText;
    public TMP_Text triesText;
    public TMP_Text cheatsText;

    //setter for score element values
    public void NewScoreElement (string username, int lessons, int tries, int cheats){

        usernameText.text = username;
        lessonsText.text = lessons.ToString();
        triesText.text = tries.ToString();
        cheatsText.text = cheats.ToString();
    }

}