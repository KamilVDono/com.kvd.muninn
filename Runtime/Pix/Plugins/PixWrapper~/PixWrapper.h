#pragma once
#include "UnityColor.h"

#define DLLExport __declspec(dllexport)

extern "C"
{
	DLLExport void StartEvent(const UnityColor* const color, const wchar_t* eventName);
	DLLExport void EndEvent();
	DLLExport void SetMarker(const UnityColor* const color, const wchar_t* marker);
	DLLExport void ReportCounter(const wchar_t* counter, float value);
}