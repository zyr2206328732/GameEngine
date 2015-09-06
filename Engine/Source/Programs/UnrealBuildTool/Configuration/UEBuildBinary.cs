// Copyright 1998-2015 Epic Games, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Globalization;
using Tools.DotNETCommon.CaselessDictionary;

namespace UnrealBuildTool
{	
	/// <summary>
	/// All binary types generated by UBT
	/// </summary>
	public enum UEBuildBinaryType
	{
		Executable,
		DynamicLinkLibrary,
		StaticLibrary,
		Object,
		PrecompiledHeader
	}

	/// <summary>
	/// UEBuildBinary configuration
	/// Configuration class for a UEBuildBinary.
	/// Exposes the configuration values of the BuildBinary class without exposing the functions.
	/// </summary>
	public class UEBuildBinaryConfiguration
	{
		/// <summary>
		/// The type of binary to build
		/// </summary>
		public UEBuildBinaryType Type;

		/// <summary>
		/// The output file path. This must be set before a binary can be built using it.
		/// </summary>
		public List<FileReference> OutputFilePaths = new List<FileReference>();

		/// <summary>
		/// Returns the OutputFilePath if there is only one entry in OutputFilePaths
		/// </summary>
		public FileReference OutputFilePath
		{
			get
			{
				if (OutputFilePaths.Count != 1)
				{
					throw new BuildException("Attempted to use UEBuildBinaryConfiguration.OutputFilePath property, but there are multiple (or no) OutputFilePaths. You need to handle multiple in the code that called this (size = {0})", OutputFilePaths.Count);
				}
				return OutputFilePaths[0];
			}
		}

		/// <summary>
		/// Original output filepath. This is the original binary name before hot-reload suffix has been appended to it.
		/// </summary>
		public List<FileReference> OriginalOutputFilePaths;

		/// <summary>
		/// Returns the OriginalOutputFilePath if there is only one entry in OriginalOutputFilePaths
		/// </summary>
		public FileReference OriginalOutputFilePath
		{
			get
			{
				if (OriginalOutputFilePaths.Count != 1)
				{
					throw new BuildException("Attempted to use UEBuildBinaryConfiguration.OriginalOutputFilePath property, but there are multiple (or no) OriginalOutputFilePaths. You need to handle multiple in the code that called this (size = {0})", OriginalOutputFilePaths.Count);
				}
				return OriginalOutputFilePaths[0];
			}
		}

		/// <summary>
		/// The intermediate directory for this binary. Modules should create separate intermediate directories below this. Must be set before a binary can be built using it.
		/// </summary>
		public DirectoryReference IntermediateDirectory;

		/// <summary>
		/// If true, build exports lib
		/// </summary>
		public bool bAllowExports = false;
		
		/// <summary>
		/// If true, create a separate import library
		/// </summary>
		public bool bCreateImportLibrarySeparately = false;

		/// <summary>
		/// If true, include dependent libraries in the static library being built
		/// </summary>
		public bool bIncludeDependentLibrariesInLibrary = false;

		/// <summary>
		/// If false, this binary will not be compiled and it is only used to set up link environments
		/// </summary>
		public bool bAllowCompilation = true;

		/// <summary>
		/// True if this binary has any Build.cs files, if not this is probably a binary-only plugins
		/// </summary>
		public bool bHasModuleRules = true;

        /// <summary>
        /// For most binaries, this is false. If this is a cross-platform binary build for a specific platform (for example XB1 DLL for a windows editor) this will be true.
        /// </summary>
        public bool bIsCrossTarget = false;

        /// <summary>
		/// If true, creates an additional console application. Hack for Windows, where it's not possible to conditionally inherit a parent's console Window depending on how
		/// the application is invoked; you have to link the same executable with a different subsystem setting.
		/// </summary>
		public bool bBuildAdditionalConsoleApp = false;

		/// <summary>
		/// The projectfile path
		/// </summary>
		public FileReference ProjectFilePath;
		
		/// <summary>
		/// List of modules to link together into this executable
		/// </summary>
		public List<string> ModuleNames = new List<string>();

		/// <summary>
		/// The configuration class for a binary build.
		/// </summary>
		/// <param name="InType"></param>
		/// <param name="InOutputFilePath"></param>
		/// <param name="bInAllowExports"></param>
		/// <param name="bInCreateImportLibrarySeparately"></param>
        /// <param name="bInIsCrossTarget">For most binaries, this is false. If this is a cross-platform binary build for a specific platform (for example XB1 DLL for a windows editor) this will be true.</param>
        /// <param name="InProjectFilePath"></param>
		/// <param name="InModuleNames"></param>
		public UEBuildBinaryConfiguration(
				UEBuildBinaryType InType,
				IEnumerable<FileReference> InOutputFilePaths = null,
				DirectoryReference InIntermediateDirectory = null,
				bool bInAllowExports = false,
				bool bInCreateImportLibrarySeparately = false,
				bool bInIncludeDependentLibrariesInLibrary = false,
				bool bInAllowCompilation = true,
				bool bInHasModuleRules = true,
                bool bInIsCrossTarget = false,
				FileReference InProjectFilePath = null,
				IEnumerable<string> InModuleNames = null
			)
		{
			Type = InType;
			if (InOutputFilePaths != null)
			{
				OutputFilePaths.AddRange(InOutputFilePaths);
			}
			IntermediateDirectory = InIntermediateDirectory;
			bAllowExports = bInAllowExports;
			bCreateImportLibrarySeparately = bInCreateImportLibrarySeparately;
			bIncludeDependentLibrariesInLibrary = bInIncludeDependentLibrariesInLibrary;
			bAllowCompilation = bInAllowCompilation;
			bHasModuleRules = bInHasModuleRules;
            bIsCrossTarget = bInIsCrossTarget;
			ProjectFilePath = InProjectFilePath;
			if(InModuleNames != null)
			{
				ModuleNames.AddRange(InModuleNames);
			}
		}
	}


	/// <summary>
	/// A binary built by UBT.
	/// </summary>
	public abstract class UEBuildBinary
	{
		/// <summary>
		/// The target which owns this binary.
		/// </summary>
		public UEBuildTarget Target;

		/// <summary>
		/// The build binary configuration data
		/// </summary>
		public UEBuildBinaryConfiguration Config = null;

		/// <summary>
		/// Create an instance of the class with the given configuration data
		/// </summary>
		/// <param name="InConfig">The build binary configuration to initialize the class with</param>
		public UEBuildBinary( UEBuildTarget InTarget, UEBuildBinaryConfiguration InConfig)
		{
			Debug.Assert(InConfig.OutputFilePath != null && InConfig.IntermediateDirectory != null);
			Target = InTarget;
			Config = InConfig;
		}

		/// <summary>
		/// Called to resolve module names and uniquely bind modules to a binary.
		/// </summary>
		/// <param name="BuildTarget">The build target the modules are being bound for</param>
		/// <param name="Target">The target info</param>
		public virtual void BindModules() {}

		/// <summary>
		/// Builds the binary.
		/// </summary>
		/// <param name="ToolChain">The toolchain which to use for building</param>
		/// <param name="CompileEnvironment">The environment to compile the binary in</param>
		/// <param name="LinkEnvironment">The environment to link the binary in</param>
		/// <returns></returns>
		public abstract IEnumerable<FileItem> Build(IUEToolChain ToolChain, CPPEnvironment CompileEnvironment,LinkEnvironment LinkEnvironment);

		/// <summary>
		/// Called to allow the binary to modify the link environment of a different binary containing 
		/// a module that depends on a module in this binary. */
		/// </summary>
		/// <param name="DependentLinkEnvironment">The link environment of the dependency</param>
		public virtual void SetupDependentLinkEnvironment(LinkEnvironment DependentLinkEnvironment) {}

		/// <summary>
		/// Called to allow the binary to to determine if it matches the Only module "short module name".
		/// </summary>
		/// <param name="OnlyModules"></param>
		/// <returns>The OnlyModule if found, null if not</returns>
        public virtual OnlyModule FindOnlyModule(List<OnlyModule> OnlyModules)
        {
            return null;
        }

		/// <summary>
		/// Called to allow the binary to find game modules.
		/// </summary>
		/// <param name="OnlyModules"></param>
		/// <returns>The OnlyModule if found, null if not</returns>
		public virtual List<UEBuildModule> FindGameModules()
		{
			return null;
		}

		/// <summary>
		/// Generates a list of all modules referenced by this binary
		/// </summary>
		/// <param name="bIncludeDynamicallyLoaded">True if dynamically loaded modules (and all of their dependent modules) should be included.</param>
		/// <param name="bForceCircular">True if circular dependencies should be process</param>
		/// <returns>List of all referenced modules</returns>
		public virtual List<UEBuildModule> GetAllDependencyModules( bool bIncludeDynamicallyLoaded, bool bForceCircular )
		{
			return new List<UEBuildModule>();
		}

		/// <summary>
		/// Process all modules that aren't yet bound, creating binaries for modules that don't yet have one (if needed),
		/// and updating modules for circular dependencies.
		/// </summary>
		public virtual void ProcessUnboundModules()
		{
		}

		/// <summary>
		/// Sets whether to create a separate import library to resolve circular dependencies for this binary
		/// </summary>
		/// <param name="bInCreateImportLibrarySeparately">True to create a separate import library</param>
		public virtual void SetCreateImportLibrarySeparately( bool bInCreateImportLibrarySeparately )
		{
		}

		/// <summary>
		/// Sets whether to include dependent libraries when building a static library
		/// </summary>
		/// <param name="bInIncludeDependentLibrariesInLibrary">True to include dependent libraries</param>
		public virtual void SetIncludeDependentLibrariesInLibrary(bool bInIncludeDependentLibrariesInLibrary)
		{
		}

		/// <summary>
		/// Adds a module to the binary.
		/// </summary>
		/// <param name="ModuleName">The module to add</param>
		public virtual void AddModule( UEBuildModule Module )
		{
		}

		/// <summary>
		/// Gets all build products produced by this binary
		/// </summary>
		/// <param name="ToolChain">The platform toolchain</param>
		/// <param name="BuildProducts">Mapping of produced build product to type</param>
		public virtual void GetBuildProducts(IUEToolChain ToolChain, Dictionary<FileReference, BuildProductType> BuildProducts)
		{
			// Get the type of build products we're creating
			BuildProductType Type = BuildProductType.RequiredResource;
			switch(Config.Type)
			{
				case UEBuildBinaryType.Executable:
					Type = BuildProductType.Executable;
					break;
				case UEBuildBinaryType.DynamicLinkLibrary:
					Type = BuildProductType.DynamicLibrary;
					break;
				case UEBuildBinaryType.StaticLibrary:
					Type = BuildProductType.StaticLibrary;
					break;
			}

			// Add the primary build products
			string DebugExtension = UEBuildPlatform.GetBuildPlatform(Target.Platform).GetDebugInfoExtension(Config.Type);
			foreach (FileReference OutputFilePath in Config.OutputFilePaths)
			{
				AddBuildProductAndDebugFile(OutputFilePath, Type, DebugExtension, BuildProducts, ToolChain);
			}

			// Add the console app, if there is one
			if (Config.Type == UEBuildBinaryType.Executable && Config.bBuildAdditionalConsoleApp)
			{
				foreach (FileReference OutputFilePath in Config.OutputFilePaths)
				{
					AddBuildProductAndDebugFile(GetAdditionalConsoleAppPath(OutputFilePath), Type, DebugExtension, BuildProducts, ToolChain);
				}
			}

			// Add any extra files from the toolchain
			ToolChain.ModifyBuildProducts(this, BuildProducts);
		}

		/// <summary>
		/// Adds a build product and its associated debug file to a receipt.
		/// </summary>
		/// <param name="OutputFile">Build product to add</param>
		/// <param name="DebugExtension">Extension for the matching debug file (may be null).</param>
		/// <param name="Receipt">Receipt to add to</param>
		static void AddBuildProductAndDebugFile(FileReference OutputFile, BuildProductType OutputType, string DebugExtension, Dictionary<FileReference, BuildProductType> BuildProducts, IUEToolChain ToolChain)
		{
			BuildProducts.Add(OutputFile, OutputType);

			if(!String.IsNullOrEmpty(DebugExtension) && ToolChain.ShouldAddDebugFileToReceipt(OutputFile, OutputType))
			{
				BuildProducts.Add(OutputFile.ChangeExtension(DebugExtension), BuildProductType.SymbolFile);
			}
		}

		/// <summary>
		/// Helper function to get the console app BinaryName-Cmd.exe filename based on the binary filename.
		/// </summary>
		/// <param name="BinaryPath">Full path to the binary exe.</param>
		/// <returns></returns>
		public static FileReference GetAdditionalConsoleAppPath(FileReference BinaryPath)
		{
			return FileReference.Combine(BinaryPath.Directory, BinaryPath.GetFileNameWithoutExtension() + "-Cmd" + BinaryPath.GetExtension());
		}

		/**
		 * Checks whether the binary output paths are appropriate for the distribution
		 * level of its direct module dependencies
		 */
		public void CheckOutputDistributionLevelAgainstDependencies(Dictionary<UEBuildModule, UEBuildModuleDistribution> ModuleDistributionCache)
		{
			// Find maximum distribution level of its direct dependencies
			var DistributionLevel = UEBuildModuleDistribution.Public;
			var DependantModules = GetAllDependencyModules(false, false);
			List<string>[] DependantModuleNames = new List<string>[Enum.GetNames(typeof(UEBuildModuleDistribution)).Length];
			foreach (var Module in DependantModules)
			{
				UEBuildModuleDistribution ModuleDistributionLevel;
				if(!ModuleDistributionCache.TryGetValue(Module, out ModuleDistributionLevel))
				{
					ModuleDistributionLevel = Module.DetermineDistributionLevel();
					ModuleDistributionCache.Add(Module, ModuleDistributionLevel);
				}
				if (ModuleDistributionLevel != UEBuildModuleDistribution.Public)
				{
					// Make a list of non-public dependant modules so that exception
					// message can be more helpful
					int DistributionIndex = (int)ModuleDistributionLevel;
					if (DependantModuleNames[DistributionIndex] == null)
					{
						DependantModuleNames[DistributionIndex] = new List<string>();
					}
					DependantModuleNames[DistributionIndex].Add(Module.Name);

					DistributionLevel = Utils.Max(DistributionLevel, ModuleDistributionLevel);
				}
			}

			// Check Output Paths if dependencies shouldn't be distributed to everyone
			if (DistributionLevel != UEBuildModuleDistribution.Public)
			{
				foreach (var OutputFilePath in Config.OutputFilePaths)
				{
					var OutputDistributionLevel = UEBuildModule.GetModuleDistributionLevelBasedOnLocation(OutputFilePath.FullName);

					// Throw exception if output path is not appropriate
					if (OutputDistributionLevel < DistributionLevel)
					{
						var JoinedModuleNames = String.Join(",", DependantModuleNames[(int)DistributionLevel]);
						throw new BuildException("Output file \"{0}\" has distribution level of \"{1}\" but has direct dependencies on modules with distribution level of \"{2}\" ({3}).\nEither change to dynamic dependencies, set BinariesSubFolder/ExeBinariesSubFolder to \"{2}\" or set bOutputPubliclyDistributable to true in the target.cs file.",
							OutputFilePath, OutputDistributionLevel.ToString(), DistributionLevel.ToString(), JoinedModuleNames);
					}
				}
			}
		}
	};

	/// <summary>
	/// A binary built by UBT from a set of C++ modules.
	/// </summary>
	public class UEBuildBinaryCPP : UEBuildBinary
	{
		public readonly List<UEBuildModule> Modules = new List<UEBuildModule>();
		private bool bCreateImportLibrarySeparately;
		private bool bIncludeDependentLibrariesInLibrary;

		/// <summary>
		/// Create an instance initialized to the given configuration
		/// </summary>
		/// <param name="InConfig">The build binary configuration to initialize the instance to</param>
		public UEBuildBinaryCPP( UEBuildTarget InTarget, UEBuildBinaryConfiguration InConfig )
			: base( InTarget, InConfig )
		{
			bCreateImportLibrarySeparately = InConfig.bCreateImportLibrarySeparately;
			bIncludeDependentLibrariesInLibrary = InConfig.bIncludeDependentLibrariesInLibrary;
		}

		/// <summary>
		/// Adds a module to the binary.
		/// </summary>
		/// <param name="ModuleName">The module to add</param>
		public override void AddModule(UEBuildModule Module)
		{
			if(!Modules.Contains(Module))
			{
				Modules.Add(Module);
			}
		}

		// UEBuildBinary interface.

		/// <summary>
		/// Called to resolve module names and uniquely bind modules to a binary.
		/// </summary>
		/// <param name="BuildTarget">The build target the modules are being bound for</param>
		/// <param name="Target">The target info</param>
		public override void BindModules()
		{
			foreach(var Module in Modules)
			{
				if (Config.bHasModuleRules)
				{
					if(Module.Binary == null)
					{
						Module.Binary = this;
						Module.bIncludedInTarget = true;
					}
					else if(Module.Binary.Config.Type != UEBuildBinaryType.StaticLibrary)
					{
						throw new BuildException("Module \"{0}\" linked into both {1} and {2}, which creates ambiguous linkage for dependents.", Module.Name, Module.Binary.Config.OutputFilePath, Config.OutputFilePath);
					}
				}
			}
		}

		/// <summary>
		/// Generates a list of all modules referenced by this binary
		/// </summary>
		/// <param name="bIncludeDynamicallyLoaded">True if dynamically loaded modules (and all of their dependent modules) should be included.</param>
		/// <param name="bForceCircular">True if circular dependencies should be process</param>
		/// <returns>List of all referenced modules</returns>
		public override List<UEBuildModule> GetAllDependencyModules(bool bIncludeDynamicallyLoaded, bool bForceCircular)
		{
			var ReferencedModules = new CaselessDictionary<UEBuildModule.ModuleIndexPair>();
			foreach( var Module in Modules )
			{
				if( !ReferencedModules.ContainsKey( Module.Name ) )
				{
					ReferencedModules[ Module.Name ] = null;

					Module.GetAllDependencyModules(ReferencedModules, bIncludeDynamicallyLoaded, bForceCircular, bOnlyDirectDependencies: false);

					ReferencedModules[ Module.Name ] = new UEBuildModule.ModuleIndexPair{ Module = Module, Index = ReferencedModules.Count };
				}
			}

			return ReferencedModules.Values.OrderBy(M => M.Index).Select(M => M.Module).ToList();
		}

		/// <summary>
		/// Process all modules that aren't yet bound, creating binaries for modules that don't yet have one (if needed),
		/// and updating modules for circular dependencies.
		/// </summary>
		/// <returns>List of newly-created binaries (may be empty)</returns>
		public override void ProcessUnboundModules()
		{
			if (Config.bHasModuleRules)
			{
				// Modules may be added to this binary during this process, so don't foreach over ModuleNames
				foreach(UEBuildModule Module in Modules)
				{
					Module.RecursivelyCreateModules();
					Module.RecursivelyProcessUnboundModules();
				}
			}
		}

		/// <summary>
		/// Sets whether to create a separate import library to resolve circular dependencies for this binary
		/// </summary>
		/// <param name="bInCreateImportLibrarySeparately">True to create a separate import library</param>
		public override void SetCreateImportLibrarySeparately(bool bInCreateImportLibrarySeparately)
		{
			bCreateImportLibrarySeparately = bInCreateImportLibrarySeparately;
		}

		/// <summary>
		/// Sets whether to include dependent libraries when building a static library
		/// </summary>
		/// <param name="bInIncludeDependentLibrariesInLibrary"></param>
		public override void SetIncludeDependentLibrariesInLibrary(bool bInIncludeDependentLibrariesInLibrary)
		{
			bIncludeDependentLibrariesInLibrary = bInIncludeDependentLibrariesInLibrary;
		}

		bool IsBuildingDll(UEBuildBinaryType Type)
		{
			if (BuildConfiguration.bRunUnrealCodeAnalyzer)
			{
				return false;
			}

			return Type == UEBuildBinaryType.DynamicLinkLibrary;
		}

		bool IsBuildingLibrary(UEBuildBinaryType Type)
		{
			if (BuildConfiguration.bRunUnrealCodeAnalyzer)
			{
				return false;
			}

			return Type == UEBuildBinaryType.StaticLibrary;
		}

		/// <summary>
		/// Builds the binary.
		/// </summary>
		/// <param name="CompileEnvironment">The environment to compile the binary in</param>
		/// <param name="LinkEnvironment">The environment to link the binary in</param>
		/// <returns></returns>
		public override IEnumerable<FileItem> Build(IUEToolChain TargetToolChain, CPPEnvironment CompileEnvironment, LinkEnvironment LinkEnvironment)
		{
			// UnrealCodeAnalyzer produces output files only for a specific module.
			if (BuildConfiguration.bRunUnrealCodeAnalyzer && !(Modules.Any(x => x.Name == BuildConfiguration.UCAModuleToAnalyze)))
			{
				return new List<FileItem>();
			}

			// Setup linking environment.
			var BinaryLinkEnvironment = SetupBinaryLinkEnvironment(LinkEnvironment, CompileEnvironment);

			// Return linked files.
			return SetupOutputFiles(TargetToolChain, ref BinaryLinkEnvironment);
		}

		/// <summary>
		/// Called to allow the binary to modify the link environment of a different binary containing 
		/// a module that depends on a module in this binary.
		/// </summary>
		/// <param name="DependentLinkEnvironment">The link environment of the dependency</param>
		public override void SetupDependentLinkEnvironment(LinkEnvironment DependentLinkEnvironment)
		{
			foreach (FileReference OutputFilePath in Config.OutputFilePaths)
			{
				FileReference LibraryFileName;
				if (Config.Type == UEBuildBinaryType.StaticLibrary
					|| DependentLinkEnvironment.Config.Target.Platform == CPPTargetPlatform.Mac
					|| DependentLinkEnvironment.Config.Target.Platform == CPPTargetPlatform.Linux)
				{
					LibraryFileName = OutputFilePath;
				}
				else
				{
					LibraryFileName = FileReference.Combine(Config.IntermediateDirectory, OutputFilePath.GetFileNameWithoutExtension() + ".lib");
				}
				DependentLinkEnvironment.Config.AdditionalLibraries.Add(LibraryFileName.FullName);
			}
			
			// If we're linking against static library containing the launch module on windows, we need to add the compiled resource separately. We can't link it through the static library.
			if(Config.Type == UEBuildBinaryType.StaticLibrary && Modules.Any(x => x.Name == "Launch") && (Target.Platform == UnrealTargetPlatform.Win32 || Target.Platform == UnrealTargetPlatform.Win64))
			{
				FileReference ResourceFileRef = FileReference.Combine(Config.IntermediateDirectory, "Launch", "PCLaunch.rc.res");
				DependentLinkEnvironment.InputFiles.Add(FileItem.GetItemByFileReference(ResourceFileRef));
			}
		}

		/// <summary>
		/// Called to allow the binary to to determine if it matches the Only module "short module name".
		/// </summary>
		/// <param name="OnlyModules"></param>
		/// <returns>The OnlyModule if found, null if not</returns>
		public override OnlyModule FindOnlyModule(List<OnlyModule> OnlyModules)
		{
			foreach (var Module in Modules)
			{
				foreach (var OnlyModule in OnlyModules)
				{
					if (OnlyModule.OnlyModuleName.ToLower() == Module.Name.ToLower())
					{
						return OnlyModule;
					}
				}
			}
			return null;
		}

		public override List<UEBuildModule> FindGameModules()
		{
			var GameModules = new List<UEBuildModule>();
			foreach (var Module in Modules)
			{
				if (!Module.ModuleDirectory.IsUnderDirectory(UnrealBuildTool.EngineDirectory))
				{
					GameModules.Add(Module);
				}
			}
			return GameModules;
		}

		/// <summary>
		/// Gets all build products produced by this binary
		/// </summary>
		/// <param name="ToolChain">The platform toolchain</param>
		/// <param name="BuildProducts">Mapping of produced build product to type</param>
		public override void GetBuildProducts(IUEToolChain ToolChain, Dictionary<FileReference, BuildProductType> BuildProducts)
		{
			base.GetBuildProducts(ToolChain, BuildProducts);

			// Add the compiled resource file if we're building a static library containing the launch module on Windows
			if(Config.Type == UEBuildBinaryType.StaticLibrary && Modules.Any(x => x.Name == "Launch") && (Target.Platform == UnrealTargetPlatform.Win32 || Target.Platform == UnrealTargetPlatform.Win64))
			{
				FileReference ResourceFilePath = FileReference.Combine(Config.IntermediateDirectory, "Launch", "PCLaunch.rc.res");
				BuildProducts.Add(ResourceFilePath, BuildProductType.StaticLibrary);
			}
		}

		// Object interface.

		/// <summary>
		/// ToString implementation
		/// </summary>
		/// <returns>Returns the OutputFilePath for this binary</returns>
		public override string ToString()
		{
			return Config.OutputFilePath.FullName;
		}

		private LinkEnvironment SetupBinaryLinkEnvironment(LinkEnvironment LinkEnvironment, CPPEnvironment CompileEnvironment)
		{
			var BinaryLinkEnvironment = LinkEnvironment.DeepCopy();
			var LinkEnvironmentVisitedModules = new Dictionary<UEBuildModule, bool>();
			var BinaryDependencies = new List<UEBuildBinary>();
			CompileEnvironment.Config.bIsBuildingDLL = IsBuildingDll(Config.Type);
			CompileEnvironment.Config.bIsBuildingLibrary = IsBuildingLibrary(Config.Type);

			var BinaryCompileEnvironment = CompileEnvironment.DeepCopy();
			// @Hack: This to prevent UHT from listing CoreUObject.generated.cpp as its dependency.
			// We flag the compile environment when we build UHT so that we don't need to check
			// this for each file when generating their dependencies.
			BinaryCompileEnvironment.bHackHeaderGenerator = (Target.GetAppName() == "UnrealHeaderTool");

			// @todo: This should be in some Windows code somewhere...
			// Set the original file name macro; used in PCLaunch.rc to set the binary metadata fields.
			var OriginalFilename = (Config.OriginalOutputFilePaths != null) ?
				Config.OriginalOutputFilePaths[0].GetFileName() :
				Config.OutputFilePaths[0].GetFileName();
			BinaryCompileEnvironment.Config.Definitions.Add("ORIGINAL_FILE_NAME=\"" + OriginalFilename + "\"");

			foreach (var Module in Modules)
			{
				List<FileItem> LinkInputFiles; 
				if(Module.Binary == null || Module.Binary == this)
				{
					// Compile each module.
					Log.TraceVerbose("Compile module: " + Module.Name);
					LinkInputFiles = Module.Compile(CompileEnvironment, BinaryCompileEnvironment);

					// NOTE: Because of 'Shared PCHs', in monolithic builds the same PCH file may appear as a link input
					// multiple times for a single binary.  We'll check for that here, and only add it once.  This avoids
					// a linker warning about redundant .obj files. 
					foreach (var LinkInputFile in LinkInputFiles)
					{
						if (!BinaryLinkEnvironment.InputFiles.Contains(LinkInputFile))
						{
							BinaryLinkEnvironment.InputFiles.Add(LinkInputFile);
						}
					}
				}
				else 
				{
					BinaryDependencies.Add(Module.Binary);
				}

				if (!BuildConfiguration.bRunUnrealCodeAnalyzer)
				{
					// Allow the module to modify the link environment for the binary.
					Module.SetupPrivateLinkEnvironment(this, BinaryLinkEnvironment, BinaryDependencies, LinkEnvironmentVisitedModules);
				}
			}


			// Allow the binary dependencies to modify the link environment.
			foreach (var BinaryDependency in BinaryDependencies)
			{
				BinaryDependency.SetupDependentLinkEnvironment(BinaryLinkEnvironment);
			}

			// Remove the default resource file on Windows (PCLaunch.rc) if the user has specified their own
			if (BinaryLinkEnvironment.InputFiles.Select(Item => Path.GetFileName(Item.AbsolutePath).ToLower()).Any(Name => Name.EndsWith(".res") && !Name.EndsWith(".inl.res") && Name != "pclaunch.rc.res"))
			{
				BinaryLinkEnvironment.InputFiles.RemoveAll(x => Path.GetFileName(x.AbsolutePath).ToLower() == "pclaunch.rc.res");
			}

			// Set the link output file.
			BinaryLinkEnvironment.Config.OutputFilePaths = Config.OutputFilePaths.ToList();

			// Set whether the link is allowed to have exports.
			BinaryLinkEnvironment.Config.bHasExports = Config.bAllowExports;

			// Set the output folder for intermediate files
			BinaryLinkEnvironment.Config.IntermediateDirectory = Config.IntermediateDirectory;

			// Put the non-executable output files (PDB, import library, etc) in the same directory as the production
			BinaryLinkEnvironment.Config.OutputDirectory = Config.OutputFilePaths[0].Directory;

			// Setup link output type
			BinaryLinkEnvironment.Config.bIsBuildingDLL = IsBuildingDll(Config.Type);
			BinaryLinkEnvironment.Config.bIsBuildingLibrary = IsBuildingLibrary(Config.Type);

			return BinaryLinkEnvironment;
		}

		private List<FileItem> SetupOutputFiles(IUEToolChain TargetToolChain, ref LinkEnvironment BinaryLinkEnvironment)
		{
			// Early exits first
			if (ProjectFileGenerator.bGenerateProjectFiles)
			{
				// We're generating projects.  Since we only need include paths and definitions, there is no need
				// to go ahead and run through the linking logic.
				return BinaryLinkEnvironment.InputFiles;
			}

			if (BuildConfiguration.bEnableCodeAnalysis)
			{
				// We're only analyzing code, so we won't actually link any executables.  Instead, our output
				// files will simply be the .obj files that were compiled during static analysis.
				return BinaryLinkEnvironment.InputFiles;
			}

			if (BuildConfiguration.bRunUnrealCodeAnalyzer)
			{
				//
				// Create actions to analyze *.includes files and provide suggestions on how to modify PCH.
				//
				return CreateOutputFilesForUCA(BinaryLinkEnvironment);
			}

			//
			// Regular linking action.
			//
			var OutputFiles = new List<FileItem>();
			if (bCreateImportLibrarySeparately)
			{
				// Mark the link environment as cross-referenced.
				BinaryLinkEnvironment.Config.bIsCrossReferenced = true;

				if (BinaryLinkEnvironment.Config.Target.Platform != CPPTargetPlatform.Mac && BinaryLinkEnvironment.Config.Target.Platform != CPPTargetPlatform.Linux)
				{
					// Create the import library.
					OutputFiles.AddRange(BinaryLinkEnvironment.LinkExecutable(true));
				}
			}

			BinaryLinkEnvironment.Config.bIncludeDependentLibrariesInLibrary = bIncludeDependentLibrariesInLibrary;

			// Link the binary.
			FileItem[] Executables = BinaryLinkEnvironment.LinkExecutable(false);
			OutputFiles.AddRange(Executables);

			// Produce additional console app if requested
			if (Config.bBuildAdditionalConsoleApp)
			{
				// Produce additional binary but link it as a console app
				var ConsoleAppLinkEvironment = BinaryLinkEnvironment.DeepCopy();
				ConsoleAppLinkEvironment.Config.bIsBuildingConsoleApplication = true;
				ConsoleAppLinkEvironment.Config.WindowsEntryPointOverride = "WinMainCRTStartup";		// For WinMain() instead of "main()" for Launch module
				ConsoleAppLinkEvironment.Config.OutputFilePaths = ConsoleAppLinkEvironment.Config.OutputFilePaths.Select(Path => GetAdditionalConsoleAppPath(Path)).ToList();

				// Link the console app executable
				OutputFiles.AddRange(ConsoleAppLinkEvironment.LinkExecutable(false));
			}

			foreach (var Executable in Executables)
			{
				OutputFiles.AddRange(TargetToolChain.PostBuild(Executable, BinaryLinkEnvironment));
			}

			return OutputFiles;
		}

		private List<FileItem> CreateOutputFilesForUCA(LinkEnvironment BinaryLinkEnvironment)
		{
			var OutputFiles = new List<FileItem>();
			var ModuleName = Modules.Select(Module => Module.Name).First(Name => Name.CompareTo(BuildConfiguration.UCAModuleToAnalyze) == 0);
			var ModuleCPP = (UEBuildModuleCPP)Target.GetModuleByName(ModuleName);
			var ModulePrivatePCH = ModuleCPP.ProcessedDependencies.UniquePCHHeaderFile;
			var IntermediatePath = Path.Combine(Target.ProjectIntermediateDirectory.FullName, ModuleName);
			var OutputFileName = Target.OutputPath;
			var OutputFile = FileItem.GetItemByFileReference(OutputFileName);

			Action LinkAction = new Action(ActionType.Compile);
			LinkAction.WorkingDirectory = UnrealBuildTool.EngineSourceDirectory.FullName;
			LinkAction.CommandPath = System.IO.Path.Combine(LinkAction.WorkingDirectory, @"..", @"Binaries", @"Win32", @"UnrealCodeAnalyzer.exe");
			LinkAction.ProducedItems.Add(OutputFile);
			LinkAction.PrerequisiteItems.AddRange(BinaryLinkEnvironment.InputFiles);
			LinkAction.CommandArguments = @"-AnalyzePCHFile -PCHFile=""" + ModulePrivatePCH.AbsolutePath + @""" -OutputFile=""" + OutputFileName + @""" -HeaderDataPath=""" + IntermediatePath + @""" -UsageThreshold " + BuildConfiguration.UCAUsageThreshold.ToString(CultureInfo.InvariantCulture);

			foreach (string IncludeSearchPath in ModuleCPP.IncludeSearchPaths)
			{
				LinkAction.CommandArguments += @" /I""" + LinkAction.WorkingDirectory + @"\" + IncludeSearchPath + @"""";
			}

			OutputFiles.Add(OutputFile);

			return OutputFiles;
		}
	};

	/// <summary>
	/// A DLL built by MSBuild from a C# project.
	/// </summary>
	public class UEBuildBinaryCSDLL : UEBuildBinary
	{
		/// <summary>
		/// Create an instance initialized to the given configuration
		/// </summary>
		/// <param name="InConfig">The build binary configuration to initialize the instance to</param>
		public UEBuildBinaryCSDLL(UEBuildTarget InTarget, UEBuildBinaryConfiguration InConfig)
			: base(InTarget, InConfig)
		{
		}

		/// <summary>
		/// Builds the binary.
		/// </summary>
		/// <param name="ToolChain">The toolchain to use for building</param>
		/// <param name="CompileEnvironment">The environment to compile the binary in</param>
		/// <param name="LinkEnvironment">The environment to link the binary in</param>
		/// <returns></returns>
		public override IEnumerable<FileItem> Build(IUEToolChain ToolChain, CPPEnvironment CompileEnvironment, LinkEnvironment LinkEnvironment)
		{
			var ProjectCSharpEnviroment = new CSharpEnvironment();
			if (LinkEnvironment.Config.Target.Configuration == CPPTargetConfiguration.Debug)
			{ 
				ProjectCSharpEnviroment.TargetConfiguration = CSharpTargetConfiguration.Debug;
			}
			else
			{
				ProjectCSharpEnviroment.TargetConfiguration = CSharpTargetConfiguration.Development;
			}
			ProjectCSharpEnviroment.EnvironmentTargetPlatform = LinkEnvironment.Config.Target.Platform;

			// Currently only supported by windows...
			UEToolChain.GetPlatformToolChain(CPPTargetPlatform.Win64).CompileCSharpProject(
				ProjectCSharpEnviroment, Config.ProjectFilePath, Config.OutputFilePath);

			return new FileItem[] { FileItem.GetItemByFileReference(Config.OutputFilePath) };
		}
	};
}
