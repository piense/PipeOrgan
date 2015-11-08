#include <cstdint>

#pragma once
class OrganManual
{
public:
	OrganManual();
	~OrganManual();

	std::uint64_t currentState = 0;

	std::uint64_t allKeys = 0x00FFFFFFFFFFFFFFF0;

	//Note 24 = middle C
	std::uint64_t notes[60];

	void NoteOn(int note);
	void NoteOff(int note);
};

