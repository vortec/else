// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

// include Python.h, but undef _Debug because we don't want python debug.
#if _DEBUG
#define _DEBUG_IS_ENABLED
#undef _DEBUG
#endif
#include <Python.h>
#if defined(_DEBUG_IS_ENABLED)
#define _DEBUG
#endif


