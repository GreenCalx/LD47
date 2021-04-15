using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIRow : MonoBehaviour
{
    private UICol[] cols;
    private int n_cols;
    private int selected_col;
    public bool is_selected;

    private Text row_text;

    void Start()
    {
        cols = GetComponentsInChildren<UICol>();
        row_text = GetComponent<Text>();
        is_selected = false;
        refreshSelectedCol();
    }

    void Update()
    {
        
    }

    private void refreshSelectedCol()
    {
        selected_col = 0;
        for( int i=0; i < cols.Length; i++)
        {
            UICol col = cols[i];
            if (col.is_selected)
                selected_col = i;
        }
    }

    public void applyChoice()
    {
        foreach ( UICol col in cols )
        {
            if ( col.is_selected )
            {
                col.doAction();
            }
        }
    }

    public void changeCol(int iStep)
    {
        selected_col += iStep;

        if( selected_col < 0 )
            selected_col = cols.Length-1;
        if ( selected_col >= cols.Length )
            selected_col = 0;
        Debug.Log(selected_col);
    }

    public void unfold_refresh()
    {
        cols = GetComponentsInChildren<UICol>();

        if (is_selected)
            row_text.color = UIColor.base_color;
        else
            row_text.color = UIColor.darken_color;
            
        for( int i=0; i < cols.Length; i++)
        {
            UICol col = cols[i];
            col.is_selected = ( i == selected_col );
            col.refresh();
        }
    }

}
