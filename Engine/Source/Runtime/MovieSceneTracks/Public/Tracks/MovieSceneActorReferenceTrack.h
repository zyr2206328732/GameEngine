// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "MovieScene.h"
#include "MovieSceneTrack.h"
#include "MovieScenePropertyTrack.h"
#include "MovieSceneActorReferenceTrack.generated.h"

/**
 * Handles manipulation of actor reference properties in a movie scene
 */
UCLASS( MinimalAPI )
class UMovieSceneActorReferenceTrack : public UMovieScenePropertyTrack
{
	GENERATED_UCLASS_BODY()

public:
	/** UMovieSceneTrack interface */
	virtual UMovieSceneSection* CreateNewSection() override;
	virtual FMovieSceneEvalTemplatePtr CreateTemplateForSection(const UMovieSceneSection& InSection) const override;
};
