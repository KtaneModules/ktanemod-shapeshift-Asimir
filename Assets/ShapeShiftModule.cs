using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class ShapeShiftModule : MonoBehaviour
{
	public KMBombInfo BombInfo;
	public KMSelectable[] buttons;
	public Transform display;
	public Texture[] textures;

	int[,] nextL = new int[4, 4];
	int[,] nextR = new int[4, 4];
	bool[,] seen = new bool[4, 4];
	int startL;
	int startR;
	int solutionL;
	int solutionR;
	int currentL;
	int currentR;
	int tempL;
	int displayL;
	int displayR;

	bool isActivated = false;
	bool isSolved = false;

	int batteryCount;
	int batteryAACount;
	bool SIGon;
	bool MSAon;
	bool SNDon;
	bool INDon;
	bool CARoff;
	bool BOBoff;
	bool FRQoff;
	bool parallelPort;
	bool RJ45Port;
	bool RCAPort;
	bool DVIPort;
	bool PS2Port;
	string Serial;
	bool serialOdd;
	bool serialVowel;

    void Start() {
        Init();
        GetComponent<KMBombModule>().OnActivate += ActivateModule;
    }

    void Init() {
		buttons [0].OnInteract += delegate () {buttons[0].AddInteractionPunch(0.2f); ChangeDisplayL (); return false; };
		buttons [1].OnInteract += delegate () {buttons[1].AddInteractionPunch(1); Submit (); return false; };
		buttons [2].OnInteract += delegate () {buttons[2].AddInteractionPunch(0.2f); ChangeDisplayR (); return false; };
	}

    void ActivateModule() {

		isActivated = true;

		List<string> Response;

		batteryCount = 0;
		batteryAACount = 0;
		Response = BombInfo.QueryWidgets (KMBombInfo.QUERYKEY_GET_BATTERIES, null);
		foreach( string Value in Response ) {
			Dictionary< string,int > batteryInfo = JsonConvert.DeserializeObject <Dictionary<string,int>> (Value);
			batteryCount += batteryInfo ["numbatteries"];
			if (batteryInfo ["numbatteries"] == 2) {
				batteryAACount += 2;
			}
		}

		SIGon = false;
		MSAon = false;
		SNDon = false;
		INDon = false;
		CARoff = false;
		BOBoff = false;
		FRQoff = false;
		Response = BombInfo.QueryWidgets (KMBombInfo.QUERYKEY_GET_INDICATOR, null);
		foreach( string Value in Response ) {
			Dictionary< string,string > IndicatorInfo = JsonConvert.DeserializeObject< Dictionary< string,string >> (Value);
			if (IndicatorInfo ["on"] == "True") {
				if (IndicatorInfo ["label"] == "SIG") {
					SIGon = true;
				} else if (IndicatorInfo ["label"] == "MSA") {
					MSAon = true;
				} else if (IndicatorInfo ["label"] == "SND") {
					SNDon = true;
				} else if (IndicatorInfo ["label"] == "IND") {
					INDon = true;
				}
			} else if (IndicatorInfo ["on"] == "False") {
				if (IndicatorInfo ["label"] == "CAR") {
					CARoff = true;
				} else if (IndicatorInfo ["label"] == "BOB") {
					BOBoff = true;
				} else if (IndicatorInfo ["label"] == "FRQ") {
					FRQoff = true;
				}
			}
		}

		parallelPort = false;
		RJ45Port = false;
		RCAPort = false;
		DVIPort = false;
		PS2Port = false;
		Response = BombInfo.QueryWidgets (KMBombInfo.QUERYKEY_GET_PORTS, null);
		foreach( string Value in Response ) {
			Dictionary< string, List<string> > PortInfo = JsonConvert.DeserializeObject< Dictionary< string, List<string> >> (Value);
			foreach( string PortType in PortInfo["presentPorts"] ) {
				if (PortType == "Parallel") {
					parallelPort = true;
				} else if (PortType == "RJ45") {
					RJ45Port = true;
				} else if (PortType == "StereoRCA") {
					RCAPort = true;
				} else if (PortType == "DVI") {
					DVIPort = true;
				} else if (PortType == "PS2") {
					PS2Port = true;
				}
			}
		}
			
		Response = BombInfo.QueryWidgets (KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null);
		if (!BombInfo.IsBombPresent()) {
			Serial = "XBC124";
		} else {
			Dictionary<string,string> Dict = JsonConvert.DeserializeObject < Dictionary<string,string> > (Response [0]);
			Serial = Dict ["serial"];
		}
		if (Serial.Substring (5) == "1" || Serial.Substring (5) == "3" || Serial.Substring (5) == "5" || Serial.Substring (5) == "7" || Serial.Substring (5) == "9") {
			serialOdd = true;
		} else {
			serialOdd = false;
		}
		if (Serial.Contains ("A") || Serial.Contains ("E") || Serial.Contains ("I") || Serial.Contains ("O") || Serial.Contains ("U")) {
			serialVowel = true;
		} else {
			serialVowel = false;
		}

		if (serialOdd) {
			nextL [0, 0] = 2;
			nextR [0, 0] = 0;
		} else {
			nextL [0, 0] = 2;
			nextR [0, 0] = 1;
		}
		if (DVIPort) {
			nextL [0, 1] = 1;
			nextR [0, 1] = 2;
		} else {
			nextL [0, 1] = 3;
			nextR [0, 1] = 2;
		}
		if (MSAon) {
			nextL [0, 2] = 3;
			nextR [0, 2] = 2;
		} else {
			nextL [0, 2] = 2;
			nextR [0, 2] = 0;
		}
		if (BOBoff) {
			nextL [0, 3] = 3;
			nextR [0, 3] = 1;
		} else {
			nextL [0, 3] = 2;
			nextR [0, 3] = 2;
		}
		if (SNDon) {
			nextL [1, 0] = 1;
			nextR [1, 0] = 1;
		} else {
			nextL [1, 0] = 1;
			nextR [1, 0] = 3;
		}
		if (serialVowel) {
			nextL [1, 1] = 3;
			nextR [1, 1] = 3;
		} else {
			nextL [1, 1] = 0;
			nextR [1, 1] = 1;
		}
		if (SIGon) {
			nextL [1, 2] = 3;
			nextR [1, 2] = 3;
		} else {
			nextL [1, 2] = 0;
			nextR [1, 2] = 0;
		}
		if (batteryAACount >= 2) {
			nextL [1, 3] = 3;
			nextR [1, 3] = 0;
		} else {
			nextL [1, 3] = 0;
			nextR [1, 3] = 3;
		}
		if (CARoff) {
			nextL [2, 0] = 3;
			nextR [2, 0] = 1;
		} else {
			nextL [2, 0] = 1;
			nextR [2, 0] = 3;
		}
		if (parallelPort) {
			nextL [2, 1] = 0;
			nextR [2, 1] = 2;
		} else {
			nextL [2, 1] = 1;
			nextR [2, 1] = 0;
		}
		if (INDon) {
			nextL [2, 2] = 2;
			nextR [2, 2] = 3;
		} else {
			nextL [2, 2] = 0;
			nextR [2, 2] = 0;
		}
		if (RJ45Port) {
			nextL [2, 3] = 1;
			nextR [2, 3] = 2;
		} else {
			nextL [2, 3] = 1;
			nextR [2, 3] = 1;
		}
		if (FRQoff) {
			nextL [3, 0] = 0;
			nextR [3, 0] = 1;
		} else {
			nextL [3, 0] = 2;
			nextR [3, 0] = 2;
		}
		if (RCAPort) {
			nextL [3, 1] = 0;
			nextR [3, 1] = 2;
		} else {
			nextL [3, 1] = 1;
			nextR [3, 1] = 0;
		}
		if (PS2Port) {
			nextL [3, 2] = 2;
			nextR [3, 2] = 3;
		} else {
			nextL [3, 2] = 2;
			nextR [3, 2] = 1;
		}
		if (batteryCount >= 3) {
			nextL [3, 3] = 3;
			nextR [3, 3] = 0;
		} else {
			nextL [3, 3] = 0;
			nextR [3, 3] = 3;
		}

		startL = Random.Range (0, 4);
		startR = Random.Range (0, 4);
		currentL = startL;
		currentR = startR;
		while (!seen [currentL, currentR]) {
			seen [currentL, currentR] = true;
			tempL = nextL [currentL, currentR];
			currentR = nextR [currentL, currentR];
			currentL = tempL;
		}
		solutionL = currentL;
		solutionR = currentR;
		if (solutionL == startL && solutionR == startR) {
			for (int i = 0; i < 4; i++) {
				for (int j = 0; j < 4; j++) {
					seen [i, j] = false;
				}
			}
			startL = Random.Range (0, 4);
			startR = Random.Range (0, 4);
			currentL = startL;
			currentR = startR;
			while (!seen [currentL, currentR]) {
				seen [currentL, currentR] = true;
				tempL = nextL [currentL, currentR];
				currentR = nextR [currentL, currentR];
				currentL = tempL;
			}
			solutionL = currentL;
			solutionR = currentR;
		}

		displayL = startL;
		displayR = startR;

		UpdateDisplay (startL, startR);
	}

	void ChangeDisplayL() {
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		if (isActivated && !isSolved) {
			displayL = (displayL + 1) % 4;
			UpdateDisplay (displayL, displayR);
		}
	}

	void ChangeDisplayR() {
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		if (isActivated && !isSolved) {
			displayR = (displayR + 1) % 4;
			UpdateDisplay (displayL, displayR);
		}
	}

	void UpdateDisplay(float dL, float dR) {
		if (dL == (float)startL) {
			dL += 0.5f;
		}
		if (dR == (float)startR) {
			dR += 0.5f;
		}
		display.GetComponent<Renderer>().material.mainTexture = textures[(int)(16*dL + 2*dR)];		
	}

	void Submit() {
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		if (!isActivated) {
			GetComponent<KMBombModule>().HandleStrike();
		} else if (!isSolved) {
			if (displayL == solutionL && displayR == solutionR) {
				GetComponent<KMBombModule> ().HandlePass ();
				isSolved = true;
			} else {
				GetComponent<KMBombModule>().HandleStrike();
			}
		}
	}
}
