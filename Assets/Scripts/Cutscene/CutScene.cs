using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


    [Serializable]
    public struct ImagesAndTimers 
    {
        public Sprite image;
        public double timer;
    }

    [Serializable]
    public struct TextsAndTimers 
    {
        public string text;
        public double timer;
    }

public class CutScene : MonoBehaviour
{

    public struct TimelineElem
    {
        public CutSceneElem elem;
        public Action<CutSceneElem> action;

        public TimelineElem( CutSceneElem iElem, Action<CutSceneElem> iAction)
        {
            elem = iElem;
            action = iAction;
        }
    }

    public string scene_to_load_at_exit;
    public  ImagesAndTimers[] images_and_timers;
    public  TextsAndTimers[] texts_and_timers;

    [HideInInspector]
    public Tuple<double, TimelineElem> curr_elem;

    private Dictionary<Sprite, double> dic_images_and_timers;
    private Dictionary<string, double> dic_texts_and_timers;

    private List<CutSceneElem>  elems;
    private List<CutSceneFX>    specials;

    private static Image ui_image;
    private static Text  ui_text;

    // each time has a set of action to perform ?
    private List<Tuple<double, TimelineElem>> cutscene_timeline;
    private IEnumerator<Tuple<double, TimelineElem>> cutscene_iter;
    private double cutscene_duration;
    private double cutscene_timer;

    // Start is called before the first frame update
    void Start()
    {
        if ( (scene_to_load_at_exit==null) || (scene_to_load_at_exit.Length==0) )
            Debug.LogError("Missing exit scene name in CutsceneController.");

        buildDico();
        construct_elems();
        buildCutSceneTimeline();

        ui_image    = GetComponentInChildren<Image>();
        ui_text     = GetComponentInChildren<Text>();
    }

    public void launch()
    {
        cutscene_timer = 0;
        cutscene_iter = cutscene_timeline.GetEnumerator();
        loadNext(); // Loads first elem with iterator
    }

    public void loadNext()
    {
        Debug.Log("Load next cutscene elem");
        if (!cutscene_iter.MoveNext())
        {
            exit_cutscene();
            return;
        }
        curr_elem = cutscene_iter.Current;

        curr_elem.Item2.action.DynamicInvoke(curr_elem.Item2.elem);
    }

    public void exit_cutscene()
    {
        SceneManager.LoadScene( scene_to_load_at_exit, LoadSceneMode.Single);
    }

    private static void callElem( CutSceneElem iCSE )
    {
        Debug.Log("unspecified cutscene elem of timer " + iCSE.timer );
    }

    private static void callText( CutSceneElem iCSE )
    {
        Debug.Log("callText on elem of timer " + iCSE.timer );
        CutSceneText as_txt = (CutSceneText)iCSE;
        ui_text.text = as_txt.text;

    }

    private static void callImage( CutSceneElem iCSE )
    {
        Debug.Log("callImage on elem of timer " + iCSE.timer );
        CutSceneImage as_img = (CutSceneImage)iCSE;
        ui_image.sprite = as_img.image;
    }


    private void buildCutSceneTimeline()
    {
        cutscene_timeline = new List<Tuple<double, TimelineElem>>();

        // Find total duration for cutscene
        cutscene_duration = 0;

        double cutscene_txt_duration = 0; 
        double cutscene_img_duration = 0; 
        Action<CutSceneElem> ces_delegate;
        foreach( CutSceneElem cse in elems )
        {
            ces_delegate = callElem;
            if ( cse is CutSceneImage )
            {
                ces_delegate = callImage;
                cutscene_img_duration += cse.timer;
                TimelineElem timeline_elem = new TimelineElem( cse, ces_delegate);

                cutscene_timeline.Add( new Tuple<double, TimelineElem>(cse.timer, timeline_elem));
            } else if ( cse is CutSceneText )
            {
                ces_delegate = callText;
                cutscene_txt_duration += cse.timer;
                TimelineElem timeline_elem = new TimelineElem( cse, ces_delegate);

                cutscene_timeline.Add( new Tuple<double, TimelineElem>(cse.timer, timeline_elem));
            } else {
                Debug.LogError("Unsupported cutscene elem type detected. It will be ignored. ");
            }
        } // !fe

        // sort list by timers
        cutscene_timeline.Sort( (x,y) => x.Item1.CompareTo(y.Item1) );
    }

    private void buildDico()
    {
        // images
        dic_images_and_timers = new Dictionary<Sprite, double>(images_and_timers.Length);
        for (int i=0; i < images_and_timers.Length; i++)
        { 
            ImagesAndTimers im_and_time = images_and_timers[i];
            dic_images_and_timers.Add(im_and_time.image, im_and_time.timer); 
        }

        // texts
        dic_texts_and_timers = new Dictionary<string, double>(texts_and_timers.Length);
        for (int i=0; i < texts_and_timers.Length; i++)
        { 
            TextsAndTimers txt_and_time = texts_and_timers[i];
            dic_texts_and_timers.Add(txt_and_time.text, txt_and_time.timer); 
        }
    }

    private void construct_elems()
    {
        elems = new List<CutSceneElem>();
        foreach( Sprite sprite in dic_images_and_timers.Keys )
        {
            double timer = 0f;
            if (! dic_images_and_timers.TryGetValue(sprite, out timer) )
            { Debug.LogError("Failed to retrieved timer attached to image in cutscene construct_elems"); continue; }
            elems.Add( new CutSceneImage(sprite, timer) );
        }
        foreach( string str in dic_texts_and_timers.Keys )
        {
            double timer = 0f;
            if (! dic_texts_and_timers.TryGetValue(str, out timer) )
            { Debug.LogError("Failed to retrieved timer attached to text in cutscene construct_elems"); continue; }
            elems.Add( new CutSceneText(str, timer) );
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
