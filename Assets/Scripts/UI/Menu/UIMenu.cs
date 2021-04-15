using System.Collections;
using System.Collections.Generic;
using UnityEngine;


struct UIColor {
    public static Color base_color = Color.white;
    public static Color darken_color = new Color( 1, 1, 1, 0.2f);
}

public class UIMenu : MonoBehaviour
{
    private UIRow[] rows;
    private int n_rows = 0;
    private int selected_row;
    public List<UIMenuSubscriber> subscribers;

    void Awake()
    {
        subscribers = new List<UIMenuSubscriber>(0);
    }
    void Start()
    {
        rows = GetComponentsInChildren<UIRow>();
        n_rows = rows.Length;
        selected_row = 0;
    }

    public void subscribe(in UIMenuSubscriber iSubscriber)
    {
        subscribers.Add(iSubscriber);
        iSubscriber.is_active = false;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            quit();
        
        var right   = Input.GetButtonDown("Right") ;
        var left    = Input.GetButtonDown("Left")  ;
        var down    = Input.GetButtonDown("Down") ;
        var up      = Input.GetButtonDown("Up")  ;
        var enter   = Input.GetButtonDown("Submit");
        if ( up )
        {
            selected_row = ( selected_row <= 0 ) ? 
                             n_rows         : 
                             selected_row - 1;
        }
        else if ( down )
        {
            selected_row = ( selected_row >= n_rows ) ? 
                            0 : 
                            selected_row + 1;
        }
        else if (left)
        {
            rows[selected_row].changeCol(-1);
        } 
        else if (right)
        {
            rows[selected_row].changeCol(1);
        }

        if (enter)
            applyAllChoices();

        // TODO FIXME
        // Unfold refresh every update, can be optimized.
        // we also refresh the rows component to ensure that
        // even if this script is last to init, we hjave all 
        // rows..........
        unfold_refresh_rows();
    }

    public void applyAllChoices()
    {
        foreach( UIRow row in rows)
        {
            if ( row.is_selected )
            { row.applyChoice(); break; }
        }
    }

    public void unfold_refresh_rows()
    {
        rows = GetComponentsInChildren<UIRow>();
        n_rows = rows.Length - 1;
        for( int i = 0; i < rows.Length; i++ )
        {
            UIRow row = rows[i];
            row.is_selected = (i==selected_row);
            row.unfold_refresh();
        }
    } 

    public void quit()
    {
        foreach( UIMenuSubscriber sub in subscribers)
        {
            sub.is_active = true;
        }
        Destroy(this.gameObject);
    }
    
}
