using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassChoose : MonoBehaviour
{
    public void Druid_Class()
    {
        PlayerController.class_number = 1;
    }

    public void Cleric_Class()
    {
        PlayerController.class_number = 2;
    }

    public void Wizard_Class()
    {
        PlayerController.class_number = 3;
    }
    
    public void Bard_Class()
    {
        PlayerController.class_number = 4;
    }
}