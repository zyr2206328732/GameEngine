// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

#pragma once
#include "Materials/MaterialExpression.h"
#include "MaterialExpressionArccosine.generated.h"

UCLASS(collapsecategories, hidecategories=Object)
class UMaterialExpressionArccosine : public UMaterialExpression
{
	GENERATED_UCLASS_BODY()

	UPROPERTY()
	FExpressionInput Input;

	//~ Begin UMaterialExpression Interface
#if WITH_EDITOR
	virtual int32 Compile(class FMaterialCompiler* Compiler, int32 OutputIndex) override;
	virtual void GetCaption(TArray<FString>& OutCaptions) const override;
	virtual FText GetKeywords() const override {return FText::FromString(TEXT("acos"));}
#endif
	//~ End UMaterialExpression Interface
};
