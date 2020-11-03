using System.Collections;
using System.Collections.Generic;
using System;

public static class DialogBank
{

    public static readonly string[] PSUDUDE_DIALOG= 
    {
    "Hello there newborn",
    "Time goes weird in this place",
    "My job is to welcome new data like you",
    "You need ",
    "004000",
    };


    private static List<string[]> bank;

    static DialogBank()
    {
        // load dialog in bank
        bank = new List<string[]>{
             PSUDUDE_DIALOG 
             };
    }

    public static string[] load(int id)
    {
        if ((id < 0) || (id > bank.Count))
            return new string[]{};
        return bank[id] ;
    }

}
