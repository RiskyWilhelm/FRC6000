using System;
using System.IO;
using Cysharp.Text;
using Newtonsoft.Json;
using UnityEngine;

// Copyright belongs to: https://github.com/shapedbyrainstudios/save-load-system
// Save&Load
public static class IOUtils
{
	public const string encryptionWord = "FRC";
	public const string backupExtension = ".backup";


	/// <summary> Replaces '/' with <see cref="Path.DirectorySeparatorChar"/> </summary>
	public static string FixPathByCorrectDirectorySeperator(string path)
	{
		return path.Replace('/', Path.DirectorySeparatorChar);
	}

	/// <inheritdoc cref="FixPathByCorrectDirectorySeperator(string)"/>
	public static string FixPathByCorrectDirectorySeperator(ref string path)
		=> path = FixPathByCorrectDirectorySeperator(path);

	/// <summary> Uses Newtonsoft JSON to deserialize </summary>
	public static bool Load<LoadObjectType>(string fullPathWithExtension, out LoadObjectType loadedData, bool useDecryption = false, bool allowRestoreFromBackup = true)
	{
		loadedData = default;
		FixPathByCorrectDirectorySeperator(ref fullPathWithExtension);

		// Try to load backup if file does not exists
		if (!File.Exists(fullPathWithExtension))
		{
			Debug.LogError($"Failed to load file at path: {fullPathWithExtension} File does not exists");
			return allowRestoreFromBackup && TrySaveRollbackAsMainFile(fullPathWithExtension) && Load<LoadObjectType>(fullPathWithExtension, out loadedData, useDecryption, false);
		}

		// load the serialized data from the file
		try
		{
			string dataToLoad = "";
			using (var stream = new FileStream(fullPathWithExtension, FileMode.Open))
			{
				using var reader = new StreamReader(stream);
				dataToLoad = reader.ReadToEnd();
			}

			if (useDecryption)
				dataToLoad = EncryptDecrypt(dataToLoad);

			loadedData = JsonConvert.DeserializeObject<LoadObjectType>(dataToLoad);
			return true;
		}
		// Try to load backup if any exists
		catch (Exception e)
		{
			if (allowRestoreFromBackup)
			{
				Debug.LogWarning($"Failed to load file at path: {fullPathWithExtension} Attempting to rollback. Error occured: {e}");

				if (TrySaveRollbackAsMainFile(fullPathWithExtension) && Load<LoadObjectType>(fullPathWithExtension, out loadedData, useDecryption, false))
					return true;
			}
				
			// if we hit here, one possibility is that the backup file is also corrupt
			Debug.LogError($"Failed to load file at path: {fullPathWithExtension} and backup did not work. Maybe the file is corrupted. Error occured: {e}");
		}

		return false;
	}

	/// <summary> Uses Newtonsoft JSON to serialize </summary>
	public static void Save<SaveObjectType>(SaveObjectType data, string fullPathWithExtension, bool useEncryption = false, bool createBackup = true)
	{
		FixPathByCorrectDirectorySeperator(ref fullPathWithExtension);
		string backupFilePath = ZString.Concat(fullPathWithExtension, backupExtension);

		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fullPathWithExtension));
			string dataToStore = JsonConvert.SerializeObject(data);

			if (useEncryption)
				dataToStore = EncryptDecrypt(dataToStore);

			// write the serialized data to the file
			using (var stream = new FileStream(fullPathWithExtension, FileMode.Create))
			{
				using var writer = new StreamWriter(stream);
				writer.Write(dataToStore);
			}

			// verify the newly saved file can be loaded successfully
			// if the data can be verified, back it up
			if (Load<SaveObjectType>(fullPathWithExtension, out _, useEncryption, false) && createBackup)
				File.Copy(fullPathWithExtension, backupFilePath, true);
			else
				throw new Exception($"Save file could not be verified and backup could not be created at path: {fullPathWithExtension}");
		}
		catch (Exception e)
		{
			Debug.LogError($"Error occured when trying to save data to file at path: {fullPathWithExtension} Error occured: {e}");
		}
	}

	/// <summary> Simple implementation of XOR encryption </summary>
	private static string EncryptDecrypt(string data)
	{
		using var stringBuilder = ZString.CreateStringBuilder();
		stringBuilder.TryGrow(data.Length);

		for (int i = 0; i < data.Length; i++)
			stringBuilder.Append((char)(data[i] ^ encryptionWord[i % encryptionWord.Length]));

		return stringBuilder.ToString();
	}

	private static bool TrySaveRollbackAsMainFile(string fullPathWithExtension)
	{
		string backupFilePath = ZString.Concat(fullPathWithExtension, backupExtension);
		FixPathByCorrectDirectorySeperator(ref backupFilePath);

		try
		{
			if (!File.Exists(backupFilePath))
			{
				Debug.LogError("Tried to Rollback but no backup file exists to roll back to.");
				return false;
			}
			
			File.Copy(backupFilePath, fullPathWithExtension, true);
			Debug.Log($"Saved backup as main file to: {fullPathWithExtension}");
			return true;
		}
		catch (Exception e)
		{
			Debug.LogError($"Failed to Rollback when trying to roll back to backup file at: {backupFilePath} Error Occured: {e}");
			return false;
		}
	}
}