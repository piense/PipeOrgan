// PipeOrganTranslator.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "PipeOrgan.h"

#include <mmsystem.h>

#pragma comment(lib, "winmm.lib")

PipeOrgan organ;

bool txSwell;
bool txChoir;
bool txGreat;
bool txSolo;

void CALLBACK midiInputCallback(HMIDIIN hMidiIn, UINT wMsg, DWORD_PTR dwInstance, DWORD_PTR dwParam1, DWORD_PTR dwParam2) {
	if (wMsg == MIM_DATA){
		int status = LOBYTE(dwParam1) & 0xF0;
		int channel = LOBYTE(dwParam1) & 0x0F;
		int data1 = HIBYTE(dwParam1);
		int data2 = HIBYTE(dwParam1 >> 8);
		printf("Channel: %d ", channel);
		switch (status)
		{
			case 0x80: printf("Note Off %d %d\n", data1, data2); if(txSwell) organ.Swell.NoteOff(data1); organ.SendManuals(); break;
			case 0x90: printf("Note On %d %d\n", data1, data2); organ.Swell.NoteOn(data1); organ.SendManuals(); break;
			case 0xA0: printf("Polyphonic Aftertouch\n"); break;
			case 0xB0: printf("Control Change\n"); break;
			case 0xC0: printf("Program Change\n"); break;
			case 0xD0: printf("Channel Pressure\n"); break;
			case 0xE0: printf("Pitch Bend Change\n"); break;
		}
	}
}

int _tmain(int argc, _TCHAR* argv[])
{
	//PipeOrgan pipeOrgan;
	//pipeOrgan.Swell.C0 = true;

	//pipeOrgan.SendManuals();

	unsigned int numDevs = midiInGetNumDevs();
	printf("%d MIDI devices connected\n", numDevs);
	MIDIINCAPS inputCapabilities;

	for (unsigned int i = 0; i < numDevs; i++) {
		midiInGetDevCaps(i, &inputCapabilities, sizeof(inputCapabilities));
		printf("[%d] %s\n", i, inputCapabilities.szPname);
	}

	LPHMIDIIN device = new HMIDIIN[numDevs];
	int portID = 0;
	int flag = midiInOpen(&device[portID], portID, (DWORD)&midiInputCallback, 0, CALLBACK_FUNCTION);
	if (flag != MMSYSERR_NOERROR) {
		printf("Error opening MIDI port.\n");
		return 1;
	}
	else {
		printf("You are now connected to port %d\n", portID);
		midiInStart(device[portID]);
	}
	while (1) {}

	printf("Hello World.\n");
	getchar();
	return 0;
}

