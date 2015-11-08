#include "stdafx.h"
#include "PipeOrgan.h"

PipeOrgan::PipeOrgan()
{
}

PipeOrgan::~PipeOrgan()
{
}

void PipeOrgan::SendManuals()
{
	Networking test;

	char sendBuf[] = { 0x0f, 0xe0, 0x0f, 0xe0, 0x00, 0x5c, 0x00, 0x01, 0x00,
		0x04, 0x00, 0x50, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x01, 0x00, 0x10, 0x00, 0x01, 0xf0, 0x0f, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x03, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00,
		0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	std::uint64_t solo = htonll(Solo.currentState);
	std::uint64_t swell = htonll(Swell.currentState);
	std::uint64_t great = htonll(Great.currentState);
	std::uint64_t choir = htonll(Choir.currentState);

	memcpy(sendBuf + 37, &solo, 8);
	memcpy(sendBuf + 77, &swell, 8);
	memcpy(sendBuf + 17, &great, 8);
	memcpy(sendBuf + 57, &choir, 8);
	//memcpy(sendBuf + 0, (void *)Pedal.GetState(), 8);

	test.sendData("192.168.0.3", sendBuf, sizeof(sendBuf));

}

void PipeOrgan::simpleSend()
{
	
}