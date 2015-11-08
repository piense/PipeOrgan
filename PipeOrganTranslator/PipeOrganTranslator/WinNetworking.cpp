#include "stdafx.h"
#include "WinNetworking.h"

Networking::Networking()
{
	struct addrinfo *result = NULL, *ptr = NULL, hints;
	int iResult;

	iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (iResult != 0){
		printf("WSAStartup failed with error: %d\n", iResult);
		return;
	}

	ZeroMemory(&hints, sizeof(hints));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_RAW;
	hints.ai_protocol = IPPROTO_RAW;
	
	iResult = getaddrinfo("192.168.0.254", "2000", &hints, &result);
	if (iResult != 0){
		printf("getaddrinfo failed with error: %d\n", iResult);
		WSACleanup();
		return;
	}

	for (ptr = result; ptr != NULL; ptr = ptr->ai_next)
	{

		ConnectSocket = socket(AF_INET, SOCK_RAW, IPPROTO_RAW);
		if (ConnectSocket == INVALID_SOCKET){
			printf("socket failed with error: %ld\n", WSAGetLastError());
			WSACleanup();
			return;
		}

		iResult = bind(ConnectSocket, (SOCKADDR *)ptr->ai_addr, ptr->ai_addrlen);
		if (iResult == SOCKET_ERROR)
		{ 
			printf("Bind failed with error: %d\n", WSAGetLastError());
			continue;
		}

		DWORD one = 1;
		iResult = setsockopt(ConnectSocket, IPPROTO_IP, IP_HDRINCL, (const char*)&one, sizeof(one));
		if (iResult < 0){
			printf("Header include option failed: %ld\n", WSAGetLastError());
			WSACleanup();
			return;
		}
	}

	freeaddrinfo(result);

	if (ConnectSocket == INVALID_SOCKET){
		printf("Unable to create socket.\n");
		WSACleanup();
		return;
	}

}

void Networking::sendData(const char*dest, char* sendBuf, int bufLen)
{
	int iResult;

	sockaddr_in ManualAddr;
	ManualAddr.sin_family = AF_INET;
	ManualAddr.sin_port = 500;
	inet_pton(AF_INET, dest, &(ManualAddr.sin_addr));

	//Create a header

	char * sendBuf2;
	sendBuf2 = new char[bufLen+20];

	std::uint8_t headerSize = 0x45; //????
	std::uint8_t headerServices = 8;
	std::uint16_t headerTotalLength = 112;
	std::uint16_t headerID = 1390;
	std::uint8_t headerFlags = 0;
	std::uint8_t headerFragmentOffset = 0;
	std::uint8_t headerTtl = 10;
	std::uint8_t headerProtocol = 200;
	std::uint16_t headerChecksum = 0;

	std::uint8_t sourceIP[4] = { 192, 168, 0, 254 };
	std::uint8_t destIP[4] = { ManualAddr.sin_addr.S_un.S_un_b.s_b1, ManualAddr.sin_addr.S_un.S_un_b.s_b2, ManualAddr.sin_addr.S_un.S_un_b.s_b3, ManualAddr.sin_addr.S_un.S_un_b.s_b4 };

	sendBuf2[0] = headerSize;
	sendBuf2[1] = headerServices;
	short headerTotalLengthNO = htons(headerTotalLength);
	memcpy(sendBuf2 + 2, &headerTotalLengthNO, 2);

	short headerIDNO = htons(headerID);
	memcpy(sendBuf2 + 4, &headerIDNO, 2);

	sendBuf2[6] = headerFlags;
	sendBuf2[7] = headerFragmentOffset;
	sendBuf2[8] = headerTtl;
	sendBuf2[9] = headerProtocol;

	memcpy(sendBuf2 + 12, &sourceIP, 4);

	memcpy(sendBuf2 + 16, &destIP, 4);

	memcpy(sendBuf2 + 20, sendBuf, bufLen);

	iResult = sendto(ConnectSocket, sendBuf2, 20+bufLen, 0, (SOCKADDR *)&ManualAddr, sizeof(ManualAddr));
	if (iResult < 0){
		printf("sendto failed with error: %d\n", WSAGetLastError());
		closesocket(ConnectSocket);
		WSACleanup();
		return;
	}
}

Networking::~Networking()
{
	int iResult;

	iResult = closesocket(ConnectSocket);
	if (iResult == SOCKET_ERROR){
		printf("clossocket failed iwth error: %d\n", WSAGetLastError());
		WSACleanup();
		return;
	}
	WSACleanup();
}
