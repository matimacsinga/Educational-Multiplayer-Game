using System.Collections.Generic;
using UnityEngine;

//slides that will be displayed for the quiz minigame evaluation
[System.Serializable]
public class FirstMinigameSlides{

    [SerializeField]
    List<GameObject> slides;

    public List<GameObject> Slides{

        get {return slides;}
    }
}
