// Copyright 1998-2016 Epic Games, Inc. All Rights Reserved.

#pragma once
#include "Materials/MaterialExpression.h"
#include "MaterialExpressionTangent.generated.h"

UCLASS(collapsecategories, hidecategories=Object)
class UMaterialExpressionTangent : public UMaterialExpression
{
	GENERATED_UCLASS_BODY()

	UPROPERTY()
	FExpressionInput Input;

	UPROPERTY(EditAnywhere, Category=MaterialExpressionTangent)
	float Period;

	//~ Begin UMaterialExpression Interface
#if WITH_EDITOR
	virtual int32 Compile(class FMaterialCompiler* Compiler, int32 OutputIndex) override;
	virtual void GetCaption(TArray<FString>& OutCaptions) const override;
	virtual FText GetKeywords() const override {return FText::FromString(TEXT("tan"));}
#endif
	//~ End UMaterialExpression Interface
};
