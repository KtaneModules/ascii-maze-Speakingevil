using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ASCIIMazeScript : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombModule module;
    public KMColorblindMode cb;
    public KMSelectable[] arrows;
    public KMSelectable[] scroll;
    public KMSelectable[] submit;
    public Renderer[] leds;
    public Renderer[] arrowon;
    public Material[] ledcols;
    public Material[] arrowcols;
    public TextMesh display;
    public TextMesh[] cbtexts;

    private readonly string[] ascii = new string[256] {
    "NUL", "SOH", "STX", "ETX", "EOT", "ENQ", "ACK", "BEL", "BS", "HT", "LF", "VT", "FF", "CR", "SO", "SI", "DLE", "DC1", "DC2", "DC3", "DC4", "NAK", "SYN", "ETB", "CAN", "EM", "SUB", "ESC", "FS", "GS", "RS", "US",
    " ", "!", "\"", "#", "$", "%", "&", "'", "(", ")", "*", "+", ",", "-", ".", "/", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ":", ";", "<", "=", ">", "?",
    "@", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "[", "\\", "]", "^", "_",
    "`", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "{", "|", "}", "~", "DEL",
    "Ç", "ü", "é", "â", "ä", "à", "å", "ç", "ê", "ë", "è", "ï", "î", "ì", "Ä", "Å", "É", "æ", "Æ", "ô", "ö", "ò", "û", "ù", "ÿ", "Ö", "Ü", "ø", "£", "Ø", "×", "ƒ",
    "á", "í", "ó", "ú", "ñ", "Ñ", "ª", "º", "¿", "®", "¬", "½", "¼", "¡", "«", "»", "░", "▒", "▓", "│", "┤", "Á", "Â", "À", "©", "╣", "║", "╗", "╝", "¢", "¥", "┐",
    "└", "┴", "┬", "├", "─", "┼", "ã", "Ã", "╚", "╔", "╩", "╦", "╠", "═", "╬", "¤", "ð", "Ð", "Ê", "Ë", "È", "ı", "Í", "Î", "Ï", "┘", "┌", "█", "▄", "¦", "Ì", "▀",
    "Ó", "ß", "Ô", "Ò", "õ", "Õ", "µ", "þ", "Þ", "Ú", "Û", "Ù", "ý", "Ý", "¯", "´", "­­\u2261", "±", "‗", "¾", "¶", "§", "÷", "¸", "°", "¨", "·", "¹", "³", "²", "■", "nbsp"};
    private string[][] maze = new string[13][]
      { new string[13]{"-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"},
        new string[13]{"X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X"},
        new string[13]{"-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"},
        new string[13]{"X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X"},
        new string[13]{"-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"},
        new string[13]{"X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X"},
        new string[13]{"-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"},
        new string[13]{"X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X"},
        new string[13]{"-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"},
        new string[13]{"X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X"},
        new string[13]{"-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"},
        new string[13]{"X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X"},
        new string[13]{"-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-", "X", "-"}};
    private int[] start = new int[2];
    private int[] current = new int[2];
    private int[] exit = new int[2];
    private int currentdisp;
    private string[] detchars = new string[12];
    private string[] displist = new string[4];
    private bool[] binstring = new bool[96];
    private int[] inds = new int[4];
    private bool pressable;

    private static int moduleIDCounter = 1;
    private int moduleID;

    private void Awake()
    {
        moduleID = moduleIDCounter++;
        for (int i = 0; i < 4; i++)
        {
            inds[i] = Random.Range(0, 8);
            if (cb.ColorblindModeActive)           
                cbtexts[i].text = "KBGCRMYW"[inds[i]].ToString();           
            leds[i].material = ledcols[inds[i]];
        }
        display.text = new string[8] { "( ͡° ͜ʖ ͡°)", "ಠ_ಠ", "O=('-'Q)", "(　ﾟДﾟ)＜!!", "(⊙.☉)7", "(✖╭╮✖)", "(¬_¬)", "¯\\_(ツ)_/¯" }[Random.Range(0, 8)];
        for (int i = 0; i < 2; i++)
        {
            start[i] = Random.Range(0, 7);
            current[i] = start[i];
        }
        if (start[1] == 0)
            arrowon[0].material = arrowcols[1];
        else if (start[1] == 6)
            arrowon[2].material = arrowcols[1];
        if (start[0] == 0)
            arrowon[1].material = arrowcols[1];
        else if (start[0] == 6)
            arrowon[3].material = arrowcols[1];
        maze[2 * Random.Range(0, 7)][2 * Random.Range(0, 7)] = "+";
        while (maze.Any(r => r.Contains("-")))
        {
            int[] select = new int[2];
            for (int i = 0; i < 2; i++)
                select[i] = Random.Range(0, 7);
            while (maze[2 * select[0]][2 * select[1]] != "+")
            {
                for (int i = 0; i < 2; i++)
                    select[i] = Random.Range(0, 7);
            }
            int del = Random.Range(0, 4);
            while ((del == 0 && (select[1] == 0 || maze[2 * select[0]][(2 * select[1]) - 1] != "X" || maze[2 * select[0]][(2 * select[1]) - 2] != "-")) || (del == 1 && (select[0] == 0 || maze[(2 * select[0]) - 1][2 * select[1]] != "X" || maze[(2 * select[0]) - 2][2 * select[1]] != "-")) || (del == 2 && (select[1] == 6 || maze[2 * select[0]][(2 * select[1]) + 1] != "X" || maze[2 * select[0]][(2 * select[1]) + 2] != "-")) || (del == 3 && (select[0] == 6 || maze[(2 * select[0]) + 1][2 * select[1]] != "X" || maze[(2 * select[0]) + 2][2 * select[1]] != "-")))
                del = Random.Range(0, 4);
            switch (del)
            {
                case 0:
                    maze[2 * select[0]][(2 * select[1]) - 1] = "/";
                    maze[2 * select[0]][(2 * select[1]) - 2] = "+";
                    break;
                case 1:
                    maze[(2 * select[0]) - 1][2 * select[1]] = "/";
                    maze[(2 * select[0]) - 2][2 * select[1]] = "+";
                    break;
                case 2:
                    maze[2 * select[0]][(2 * select[1]) + 1] = "/";
                    maze[2 * select[0]][(2 * select[1]) + 2] = "+";
                    break;
                case 3:
                    maze[(2 * select[0]) + 1][2 * select[1]] = "/";
                    maze[(2 * select[0]) + 2][2 * select[1]] = "+";
                    break;
            }
            for (int i = 0; i < 13; i += 2)
                for (int j = 0; j < 13; j += 2)
                    if (maze[i][j] == "+" && (j == 0 || maze[i][j - 2] != "-") && (i == 0 || maze[i - 2][j] != "-") && (j == 12 || maze[i][j + 2] != "-") && (i == 12 || maze[i + 2][j] != "-"))
                        maze[i][j] = "o";
        }
        maze = maze.Select(i => i.Select(j => "+o/".Contains(j) ? " " : "X").ToArray()).ToArray();
        maze[2 * start[0]][2 * start[1]] = "+";
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 13; j += 2)
                for (int k = 0; k < 13; k += 2)
                    if (maze[j][k] == " " && ((k > 0 && maze[j][k - 1] != "X" && maze[j][k - 2] == "+") || (j > 0 && maze[j - 1][k] != "X" && maze[j - 2][k] == "+") || (k < 12 && maze[j][k + 1] != "X" && maze[j][k + 2] == "+") || (j < 12 && maze[j + 1][k] != "X" && maze[j + 2][k] == "+")))
                        maze[j][k] = "-";
            maze = maze.Select(m => m.Select(j => j == "+" ? "o" : j).Select(j => j == "-" ? "+" : j).ToArray()).ToArray();
        }
        string z;
        z = string.Join("", maze.Select(i => string.Join("", i.Where((x, j) => j % 2 == 0).ToArray())).Where((x, i) => i % 2 == 0).ToArray());
        for (int i = 0; i < 2; i++)
            exit[i] = Random.Range(0, 7);
        if (!z.Contains(" "))
            while (Mathf.Abs(exit[0] - start[0]) + Mathf.Abs(exit[1] - start[1]) < 6)
                for (int i = 0; i < 2; i++)
                    exit[i] = Random.Range(0, 7);
        else
            while (maze[2 * exit[0]][2 * exit[1]] == "o")
                for (int i = 0; i < 2; i++)
                    exit[i] = Random.Range(0, 7);
        maze = maze.Select(m => m.Select(j => j == "X" ? "\x25a0" : "\x25a1").ToArray()).ToArray();
        maze[2 * start[0]][2 * start[1]] = "S";
        maze[2 * exit[0]][2 * exit[1]] = "E";
        for (int i = 0; i < 2; i++)
        {
            binstring[3 * i] = start[1 - i] / 4 == 1;
            binstring[(3 * i) + 1] = (start[1 - i] / 2) % 2 == 1;
            binstring[(3 * i) + 2] = start[1 - i] % 2 == 1;
            binstring[(3 * i) + 90] = exit[1 - i] / 4 == 1;
            binstring[(3 * i) + 91] = (exit[1 - i] / 2) % 2 == 1;
            binstring[(3 * i) + 92] = exit[1 - i] % 2 == 1;
        }
        for (int k = 0; k < 2; k++)
        {
            if (inds[3 * k] / 4 == 1)
                for (int i = 0; i < 3; i++)
                    binstring[(90 * k) + i] ^= true;
            if ((inds[3 * k] / 2) % 2 == 1)
                for (int i = 3; i < 6; i++)
                    binstring[(90 * k) + i] ^= true;
            if (inds[3 * k] % 2 == 1)
                for (int i = 0; i < 3; i++)
                {
                    bool temp = binstring[(90 * k) + i];
                    binstring[(90 * k) + i] = binstring[(90 * k) + i + 3];
                    binstring[(90 * k) + i + 3] = temp;
                }
        }
        for (int i = 0; i < 7; i++)
        {
            bool[] ch = new bool[4] { (inds[1] / 2) % 2 == 0, inds[1] / 4 == 0, inds[1] % 2 == 1, i % 2 == 1};
            for (int j = 0; j < 6; j++)
                binstring[(6 * i) + j + 6] = maze[ch[0] ? 2 * i : (12 - (2 * i))][ (ch[1] ^ (ch[2] && ch[3])) ? (2 * j) + 1 : (11 - (2 * j))] == "\x25a0";
        }
        for (int i = 0; i < 6; i++)
        {
            bool[] ch = new bool[4] { (inds[2] / 2) % 2 == 0, inds[2] / 4 == 0, inds[2] % 2 == 1, i % 2 == 1};
            for (int j = 0; j < 7; j++)
                binstring[(7 * i) + j + 48] = maze[ch[0] ? (2 * i) + 1 : (11 - (2 * i))][(ch[1] ^ (ch[2] && ch[3])) ? 2 * j : (12 - (2 * j))] == "\x25a0";
        }
        for (int i = 0; i < 12; i++)
        {
            int index = 0;
            for (int j = 0; j < 8; j++)
                if (binstring[(8 * i) + j])
                    index += (int)Mathf.Pow(2, 7 - j);
            detchars[i] = ascii[index];
        }
        for (int i = 0; i < 4; i++)
        {
            displist[i] = (i == 0 ? string.Empty : "\xfffd") + string.Join("\xfffd", detchars.Where((x, j) => j >= 3 * i && j < (3 * i) + 3).ToArray()) + (i == 3 ? string.Empty : "\xfffd");
        }
        Debug.LogFormat("[ASCII Maze #{0}] The displayed ASCII characters are: {1}", moduleID, string.Join("\xfffd", detchars));
        Debug.LogFormat("[ASCII Maze #{0}] The corresponding string of bits is: {1}", moduleID, string.Join("", binstring.Select((x, i) => (i % 32 == 0 ? "\n[ASCII Maze #" + moduleID + "] " : string.Empty) + (x ? "1" : "0")).ToArray()));
        Debug.LogFormat("[ASCII Maze #{0}] The colours of the LEDs are: {1}", moduleID, string.Join(", ", inds.Select(i => new string[8] { "Black", "Blue", "Green", "Cyan", "Red", "Magenta", "Yellow", "White" }[i]).ToArray()));
        Debug.LogFormat("[ASCII Maze #{0}] The decoded bits form the maze: \n[ASCII Maze #{0}] \x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\n[ASCII Maze #{0}] {1}\n[ASCII Maze #{0}] \x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0\x25a0", moduleID, string.Join("\n[ASCII Maze #" + moduleID + "] ", maze.Select(i => "\x25a0" + string.Join(string.Empty, i) + "\x25a0").ToArray()));
        module.OnActivate = Activate;
    }

    private void Activate()
    {
        pressable = true;
        display.text = displist[0];
        foreach (KMSelectable arrow in arrows)
        {
            int k = Array.IndexOf(arrows, arrow);
            arrow.OnInteract += delegate () { if (pressable && ((k == 0 && current[1] > 0) || (k == 1 && current[0] > 0) || (k == 2 && current[1] < 6) || (k == 3 && current[0] < 6))) Traverse(k); return false; };
        }
        foreach (KMSelectable arrow in scroll)
        {
            bool k = Array.IndexOf(scroll, arrow) == 0;
            arrow.OnInteract += delegate () { if (pressable && ((k && currentdisp > 0) || (!k && currentdisp < 3))) Scroll(k); return false; };
        }
        submit[0].OnInteract += delegate () { if (pressable) StartCoroutine(Submit()); return false; };
        submit[1].OnInteract += delegate ()
        {
            if (pressable)
            {
                submit[1].AddInteractionPunch();
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submit[1].transform);
                current[0] = start[0];
                current[1] = start[1];
                Debug.LogFormat("[ASCII Maze #{0}] Reset button pressed. Returning to {1}{2}", moduleID, "ABCDEFG"[start[1]], start[0] + 1);
                Locate();
            }; return false;
        };
    }

    private void Traverse(int dir)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, arrows[dir].transform);
        arrows[dir].AddInteractionPunch(0.5f);
        switch (dir)
        {
            case 0:
                if (maze[2 * current[0]][(2 * current[1]) - 1] == "\x25a0")
                {
                    module.HandleStrike();
                    Debug.LogFormat("[ASCII Maze #{0}] Hit wall west of {1}{2}", moduleID, "ABCDEFG"[current[1]], current[0] + 1);
                }
                else
                {
                    current[1]--;
                    Locate();
                }
                break;
            case 1:
                if (maze[(2 * current[0]) - 1][2 * current[1]] == "\x25a0")
                {
                    module.HandleStrike();
                    Debug.LogFormat("[ASCII Maze #{0}] Hit wall north of {1}{2}", moduleID, "ABCDEFG"[current[1]], current[0] + 1);
                }
                else
                {
                    current[0]--;
                    Locate();
                }
                break;
            case 2:
                if (maze[2 * current[0]][(2 * current[1]) + 1] == "\x25a0")
                {
                    module.HandleStrike();
                    Debug.LogFormat("[ASCII Maze #{0}] Hit wall east of {1}{2}", moduleID, "ABCDEFG"[current[1]], current[0] + 1);
                }
                else
                {
                    current[1]++;
                    Locate();
                }
                break;
            case 3:
                if (maze[(2 * current[0]) + 1][2 * current[1]] == "\x25a0")
                {
                    module.HandleStrike();
                    Debug.LogFormat("[ASCII Maze #{0}] Hit wall south of {1}{2}", moduleID, "ABCDEFG"[current[1]], current[0] + 1);
                }
                else
                {
                    current[0]++;
                    Locate();
                }
                break;
        }
    }

    private void Locate()
    {
        arrowon[0].material = arrowcols[current[1] == 0 ? 1 : 0];
        arrowon[1].material = arrowcols[current[0] == 0 ? 1 : 0];
        arrowon[2].material = arrowcols[current[1] == 6 ? 1 : 0];
        arrowon[3].material = arrowcols[current[0] == 6 ? 1 : 0];
    }

    private void Scroll(bool left)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, left ? scroll[0].transform : scroll[1].transform);
        scroll[left ? 0 : 1].AddInteractionPunch(0.3f);
        currentdisp += left ? -1 : 1;
        arrowon[4].material = arrowcols[currentdisp == 0 ? 1 : 0];
        arrowon[5].material = arrowcols[currentdisp == 3 ? 1 : 0];
        display.text = displist[currentdisp];
    }

    private IEnumerator Submit()
    {
        pressable = false;
        submit[0].AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submit[0].transform);
        Audio.PlaySoundAtTransform("Static", transform);
        for (int i = 0; i < 60; i++)
        {
            string randdisp = string.Empty;
            for (int j = 0; j < 12; j++)
            {
                int r = Random.Range(128, 255);
                randdisp += ascii[r];
            }
            display.text = randdisp;
            yield return new WaitForSeconds(0.1f);
        }
        if (current[0] == exit[0] && current[1] == exit[1])
        {
            module.HandlePass();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            foreach (Renderer a in arrowon)
                a.material = ledcols[0];
            foreach (Renderer l in leds)
                l.material = ledcols[0];
            display.text = "CORRECT";
            display.color = new Color32(0, 255, 0, 255);
            Debug.LogFormat("[ASCII Maze #{0}] Exit found.", moduleID);
        }
        else
        {
            pressable = true;
            module.HandleStrike();
            Debug.LogFormat("[ASCII Maze #{0}] {1}{2} is not the exit. Returning to {3}{4}", moduleID, "ABCDEFG"[current[1]], current[0] + 1, "ABCDEFG"[start[1]], start[0] + 1);
            current[0] = start[0];
            current[1] = start[1];
            display.text = displist[currentdisp];
            Locate();
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} scroll <lr> [Cycles between displays] | !{0} move <udlr> [Moves around the maze] | !{0} cb [Activates colourblind mode]| !{0} submit | !{0} reset ";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        if(command == "cb")
        {
            yield return null;
            for (int i = 0; i < 4; i++)
                cbtexts[i].text = "KBGCRMYW"[inds[i]].ToString();
            yield break;
        }
        if(command == "reset")
        {
            yield return null;
            submit[1].OnInteract();
            yield break;
        }
        if (command == "submit")
        {
            yield return null;
            submit[0].OnInteract();
            yield return "solve";
            yield return "strike";
            yield break;
        }
        string[] commands = command.ToLowerInvariant().Split(' ');
        if (commands.Length == 2)
        {
            if (commands[0] == "scroll")
            {
                if (commands[1] == "l")
                {
                    yield return null;
                    scroll[0].OnInteract();
                }
                else if (commands[1] == "r")
                {
                    yield return null;
                    scroll[1].OnInteract();
                }
                else
                    yield return "sendtochaterror Invalid command: " + commands[1];
            }
            else if (commands[0] == "move")
            {
                var m = Regex.Match(commands[1], @"^\s*([lurd, ]+)\s*$");
                if (m.Success)
                {
                    for(int i = 0; i < commands[1].Length; i++)
                    {
                        yield return null;
                        yield return "strike";
                        arrows["lurd".IndexOf(commands[1][i])].OnInteract();
                    }
                }
            }
            else
                yield return "sendtochaterror Invalid command: " + commands[0];
        }
        else
            yield return "sendtochaterror Too many parameters";
    }
}
