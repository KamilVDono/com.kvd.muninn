#include <Windows.h>
#include "PixWrapper.h"
#include "pix3.h"

void StartEvent(const UnityColor* const color, const wchar_t* eventName) {
	auto rByte = static_cast<unsigned char>(color->r * 255);
	auto gByte = static_cast<unsigned char>(color->g * 255);
	auto bByte = static_cast<unsigned char>(color->b * 255);
	auto pixColor = PIX_COLOR(rByte, gByte, bByte);
	PIXBeginEvent(pixColor, eventName);
}

void EndEvent()
{
	PIXEndEvent();
}

void SetMarker(const UnityColor* const color, const wchar_t* marker)
{
	auto rByte = static_cast<unsigned char>(color->r * 255);
	auto gByte = static_cast<unsigned char>(color->g * 255);
	auto bByte = static_cast<unsigned char>(color->b * 255);
	auto pixColor = PIX_COLOR(rByte, gByte, bByte);
	PIXSetMarker(pixColor, marker);
}

void ReportCounter(const wchar_t* counter, float value)
{
	PIXReportCounter(counter, value);
}
