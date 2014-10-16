// Copyright 1998-2014 Epic Games, Inc. All Rights Reserved.

/*================================================================================
	AndroidPlatform.h: Setup for the Android platform
==================================================================================*/

#pragma once

/** Define the android platform to be the active one **/
#define PLATFORM_ANDROID				1

/**
* Android specific types
**/
struct FAndroidTypes : public FGenericPlatformTypes
{
	//typedef unsigned int				DWORD;
	//typedef size_t					SIZE_T;
	//typedef decltype(NULL)			TYPE_OF_NULL;
	typedef char16_t					CHAR16;
};

typedef FAndroidTypes FPlatformTypes;

#define MAX_PATH						PATH_MAX

// Base defines, must define these for the platform, there are no defaults
#define PLATFORM_DESKTOP				0
#define PLATFORM_64BITS					0
#define PLATFORM_CAN_SUPPORT_EDITORONLY_DATA	0

// Base defines, defaults are commented out

#define PLATFORM_LITTLE_ENDIAN						1
#define PLATFORM_SUPPORTS_PRAGMA_PACK				1
#define PLATFORM_USE_LS_SPEC_FOR_WIDECHAR			1
#define PLATFORM_HAS_BSD_TIME						1
#define PLATFORM_USE_PTHREADS						1
#define PLATFORM_MAX_FILEPATH_LENGTH				MAX_PATH
#define PLATFORM_SUPPORTS_TEXTURE_STREAMING			0
#define PLATFORM_USES_DYNAMIC_RHI					1
#define PLATFORM_REQUIRES_FILESERVER				1
#define PLATFORM_TCHAR_IS_4_BYTES					1
#define PLATFORM_HAS_NO_EPROCLIM					1
#define PLATFORM_USES_ES2							1
#define PLATFORM_HAS_BSD_SOCKET_FEATURE_IOCTL		1
#define PLATFORM_HAS_TOUCH_MAIN_SCREEN				1

// Function type macros.
#define VARARGS													/* Functions with variable arguments */
#define CDECL													/* Standard C function */
#define STDCALL													/* Standard calling convention */
#define FORCEINLINE inline __attribute__ ((always_inline))		/* Force code to be inline */
#define FORCENOINLINE __attribute__((noinline))					/* Force code to NOT be inline */

#define TEXT_HELPER(a,b)	a ## b
#define TEXT(s)				TEXT_HELPER(L, s)

#define ABSTRACT abstract

// Alignment.
#define GCC_PACK(n)			__attribute__((packed,aligned(n)))
#define GCC_ALIGN(n)		__attribute__((aligned(n)))
#define REQUIRES_ALIGNED_ACCESS 1

// Operator new/delete handling.
#define OPERATOR_NEW_THROW_SPEC throw (std::bad_alloc)
#define OPERATOR_DELETE_THROW_SPEC throw()
#define OPERATOR_NEW_NOTHROW_SPEC throw ()
#define OPERATOR_DELETE_NOTHROW_SPEC throw()
