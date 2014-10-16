// Copyright 1998-2014 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "GenericWindow.h"
#include "SharedPointer.h"
#include "AllowWindowsPlatformTypes.h"
	#include "Ole2.h"
	#include "OleIdl.h"
#include "HideWindowsPlatformTypes.h"

/**
 * A platform specific implementation of FNativeWindow.
 * Native Windows provide platform-specific backing for and are always owned by an SWindow.
 */
class CORE_API FWindowsWindow : public FGenericWindow, public IDropTarget
{
public:
	~FWindowsWindow();

	/** Win32 requirement: see CreateWindowEx and RegisterClassEx. */
	static const TCHAR AppWindowClass[];

	/** Create a new FWin32Window.
	 *
	 * @param SlateWindows		List of all top level Slate windows.  This function will add the owner window to this list.
	 * @param OwnerWindow		The SlateWindow for which we are crating a backing Win32Window
	 * @param InHInstance		Win32 application instance handle
	 * @param InParent			Parent Win32 window; usually NULL.
	 * @param bShowImmediately	True to show this window as soon as its initialized
	 */
	static TSharedRef< FWindowsWindow > Make();

	HWND GetHWnd() const;

	void Initialize( class FWindowsApplication* const Application, const TSharedRef< FGenericWindowDefinition >& InDefinition, HINSTANCE InHInstance, const TSharedPtr< FWindowsWindow >& InParent, const bool bShowImmediately );


public:

	virtual void ReshapeWindow( int32 X, int32 Y, int32 Width, int32 Height ) override;

	virtual bool GetFullScreenInfo( int32& X, int32& Y, int32& Width, int32& Height ) const override;

	virtual void MoveWindowTo ( int32 X, int32 Y ) override;

	virtual void BringToFront( bool bForce = false ) override;

	virtual void HACK_ForceToFront() override;

	virtual void Destroy() override;

	virtual void Minimize() override;

	virtual void Maximize() override;

	virtual void Restore() override;

	virtual void Show() override;

	virtual void Hide() override;

	virtual void SetWindowMode( EWindowMode::Type NewWindowMode ) override;

	virtual EWindowMode::Type GetWindowMode() const override { return WindowMode; } 

	virtual bool IsMaximized() const override;

	virtual bool IsMinimized() const override;

	virtual bool IsVisible() const override;

	virtual bool GetRestoredDimensions(int32& X, int32& Y, int32& Width, int32& Height) override;

	virtual void SetWindowFocus() override;

	virtual void SetOpacity( const float InOpacity ) override;

	virtual void Enable( bool bEnable ) override;

	virtual bool IsPointInWindow( int32 X, int32 Y ) const override;

	virtual int32 GetWindowBorderSize() const override;

	virtual int32 GetWindowTitleBarSize() const override;

	virtual void* GetOSWindowHandle() const  override { return HWnd; }

	virtual bool IsForegroundWindow() const override;

	virtual void SetText(const TCHAR* const Text) override;

	/**
	 * Slate Win32Windows implement the IDropTarget and IUnknown OLE interfaces.
	 */

	/** IUnknown : attempt to get a reference to this instance as a pointer  */
	HRESULT STDCALL QueryInterface( REFIID iid, void ** ppvObject ) override;

	/** IUnknown : Invoked by OLE when a new reference to this instance */
	ULONG STDCALL AddRef( void ) override;

	/** IUnknown : Invoked by OLE when a reference to this instance is released */
	ULONG STDCALL Release( void ) override;

	/** IDropTarget : Invoked by OLE when someone drag data our window initially */
	virtual HRESULT STDCALL DragEnter( __RPC__in_opt IDataObject *DataObjectPointer, ::DWORD KeyState, POINTL CursorPosition, __RPC__inout ::DWORD *CursorEffect) override;

	/** IDropTarget : Invoked by OLE when someone drag data over our window */
	virtual HRESULT STDCALL DragOver( ::DWORD KeyState, POINTL CursorPosition, __RPC__inout ::DWORD *CursorEffect) override;

	/** IDropTarget : Invoked by OLE when someone drag data and it exits our window or hits ESC to cancel the drag and drop. */
	virtual HRESULT STDCALL DragLeave( void ) override;

	/** IDropTarget : Invoked by OLE when someone drag data our window initially */
	virtual HRESULT STDCALL Drop( __RPC__in_opt IDataObject *DataObjectPointer, ::DWORD KeyState, POINTL CursorPosition, __RPC__inout ::DWORD *CursorEffect) override;

	/**
	 * Sets the window text - usually the title but can also be text content for things like controls
	 *
	 * @param Text	The window's title or content text
	 */
	bool IsRegularWindow() const;

	/**	Sets the window region to specified dimensions */
	void AdjustWindowRegion( int32 Width, int32 Height );

	/** @return	Gives the native window a chance to adjust our stored window size before we cache it off */
	virtual void AdjustCachedSize( FVector2D& Size ) const override;

	/** Called when our parent window is minimized (which will in turn cause us to become minimized) */
	void OnParentWindowMinimized();

	/** Called when our parent window is restored (which will in turn cause us to become restored) */
	void OnParentWindowRestored();

private:

	/**
	 * Protect the constructor; only TSharedRefs of this class can be made.
	 */
	FWindowsWindow();

	void UpdateVisibility();

	/** Creates an HRGN for the window's current region.  Remember to delete this when you're done with it using
	   ::DeleteObject, unless you're passing it to SetWindowRgn(), which will absorb the reference itself. */
	HRGN MakeWindowRegionObject() const;

private:

	FWindowsApplication* OwningApplication;

	/** WIN32 window handle */
	HWND HWnd;

	/** Store the window region size for querying whether a point lies within the window */
	int32 RegionWidth;
	int32 RegionHeight;
	
	/** The mode that the window is in (windowed, fullscreen, windowedfullscreen ) */
	EWindowMode::Type WindowMode;

	int32 OLEReferenceCount;
		
	/** The placement of the window before it entered a fullscreen state */
	WINDOWPLACEMENT PreFullscreenWindowPlacement;

	/** The placement of the window before it entered a minimized state due to its parent window being minimized */
	WINDOWPLACEMENT PreParentMinimizedWindowPlacement;

	/** Virtual width and height of the window.  This is only different than the actual width and height for
	    windows which we're trying to optimize because their size changes frequently.  We'll create a larger
		window and have Windows draw it "cropped" so that it appears smaller, rather than actually resizing
		it and incurring a GPU buffer resize performance hit */
	int32 VirtualWidth;
	int32 VirtualHeight;

	bool bIsVisible : 1;
};
