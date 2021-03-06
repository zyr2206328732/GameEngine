// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

#include "MovieScenePrivatePCH.h"
#include "MovieSceneEvalTemplate.h"
#include "MovieSceneEvalTemplateSerializer.h"

bool FMovieSceneTrackImplementationPtr::Serialize(FArchive& Ar)
{
	return SerializeEvaluationTemplate(*this, Ar);
}