using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class C4 : NetworkBehaviour
{
    [SyncVar] public bool armed;
    [SyncVar] public bool planted;
    bool countingDown;

    int time = 45;

    string minutes = "00";
    string seconds = "45";

    private void FixedUpdate()
    {
        if (armed && !countingDown)
        {
            CountDown();
            countingDown = true;
        }
    }

    void ToggleMenu(bool value)
    {
        PlayerCanvas.canvas.C4Recieve(value, armed, planted, minutes, seconds);
    }

    [Command]
    public void CmdArmed()
    {
        armed = true;
        PlayerCanvas.canvas.C4Recieve(PlayerCanvas.canvas.c4Panel.activeInHierarchy, armed, planted, minutes, seconds);
    }

    [Command]
    public void CmdPlanted()
    {
        planted = true;
        PlayerCanvas.canvas.C4Recieve(PlayerCanvas.canvas.c4Panel.activeInHierarchy, armed, planted, minutes, seconds);
    }

    void CountDown()
    {
        time--;
        if (PlayerCanvas.canvas.c4Panel.activeInHierarchy)
        {
            // Minutes and Seconds Text Formating



            // Send to Canvas
            PlayerCanvas.canvas.C4Recieve(true, armed, planted, minutes, seconds);
        }

        if (time == 0)
        {
            print("Boom");
        }

        Invoke("CountDown", 1f);
    }
}
