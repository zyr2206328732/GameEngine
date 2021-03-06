// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "ModuleInterface.h"


class IProfilerServiceManager;


/**
 * Interface for ProfilerService modules.
 */
class IProfilerServiceModule
	: public IModuleInterface
{
public:

	/** 
	 * Creates a profiler service.
	 *
	 * @param Capture Specifies whether to start capturing immediately.
	 * @return A new profiler service.
	 */
	virtual TSharedPtr<IProfilerServiceManager> CreateProfilerServiceManager() = 0;

protected:

	/** Virtual destructor */
	virtual ~IProfilerServiceModule() { }
};
