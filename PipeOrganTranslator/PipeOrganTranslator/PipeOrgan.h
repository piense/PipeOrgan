#include "OrganManual.h"
#include "OrganStops.h"
#include "WinNetworking.h"
#include <cstdint>

#pragma once
class PipeOrgan
{
private:
	void simpleSend();
	Networking netConnection;
public:
	PipeOrgan();
	~PipeOrgan();
	OrganManual Solo;
	OrganManual Swell;
	OrganManual Great;
	OrganManual Choir;
	OrganManual Pedal;
	OrganStops Stops;
	void SendManuals();
	void SendStops();
};