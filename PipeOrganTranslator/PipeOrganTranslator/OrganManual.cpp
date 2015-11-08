#include "stdafx.h"
#include "OrganManual.h"


OrganManual::OrganManual()
{
	notes[0] = 0x000000000000000010;
	for (int i = 1; i < 60; i++)
		notes[i] = notes[i - 1] * 2;
}

void OrganManual::NoteOn(int note)
{
	if (note < 36 || note > (36+60))
		return;
	currentState = currentState | notes[note - 36];
}

void OrganManual::NoteOff(int note)
{
	if (note < 36 || note > (36+60))
		return;
	currentState = currentState & (notes[note - 36]^allKeys);
}

OrganManual::~OrganManual()
{
}
