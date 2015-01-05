using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;

public class DAO <T> where T : DTO {

	public static Thread thread;
	public static List<T> loaded;

	protected string type;
	protected string path;

	public DAO() {
		type = Type.GetType (typeof(T).ToString()).ToString ();
		path = Application.persistentDataPath + "/Saved" + type + ".gd";
	}

	public DAO (string path){
		type = Type.GetType (typeof(T).ToString()).ToString ();
		this.path = path + "/Saved" + type + ".gd";
	}

	public void SaveList (List<T> tList) {
		loaded.AddRange (tList);
		List<T> file = LoadFile (); //This is slowly
		loaded = file;

		if (tList != null) {
			int index = loaded.Count + 1;
			foreach (T t in tList) {
				t.SetId (index++);
				t.SetUpdated (false);
				loaded.Add (t);
				Debug.Log ("Save " + t);
			}
			SaveFile (loaded);
		}else{

		}
	}

	public void SaveListBackground (List<T> tList) {
		loaded.AddRange (tList);
		thread = new Thread (() => this.SaveList (tList));
		thread.Start ();
	}

	public void SaveOrUpdateBackground (DTO se) {
		thread = new Thread (() => this.SaveOrUpdate (se));
		thread.Start ();
	}

	public void SaveOrUpdate(DTO se){
		loaded = LoadFile ();

		if(se != null){
			if (se.GetId () == 0) {
				//Set the id by the size of the existent amount of SerializedEntities
				se.SetId (loaded.Count + 1);
				se.SetUpdated (false);
				loaded.Add (se as T);
				SaveFile (loaded);
				////Debug.Log ("Serialized Saved [id = " + se.Id + "] " + se + " at: " + path);
			}else{
				bool updateCheck = false;
				for(int i = 0; i < loaded.Count; i++){
					if(loaded[i] != null && loaded[i].GetId () == se.GetId ()){
						updateCheck = true;
						se.SetUpdated (true);
						//se.updated = true;
						loaded[i] = se as T;
						SaveFile (loaded);
						
						////Debug.Log ("Serialized Updated [id = " + se.Id + "] " + se);
						break;
					}
				}
				if(!updateCheck){
					////Debug.Log ("Serialized Id not found!");
				}
			}

		}else{
			//Debug.Log ("No Serialized to save!");
		}
	}

	public void UpdateWithoutSetAsUpdated (DTO se) {
		loaded = LoadFile ();
		
		if(se != null){
			if (se.GetId () == 0) {
				//Set the id by the size of the existent amount of SerializedEntities
				se.SetId (loaded.Count + 1);
				se.SetUpdated (false);
				loaded.Add (se as T);
				SaveFile (loaded);
				//Debug.Log ("Serialized Saved [id = " + se.Id + "] " + se + " at: " + path);
			}else{
				bool updateCheck = false;
				for(int i = 0; i < loaded.Count; i++){
					if(loaded[i] != null && loaded[i].GetId () == se.GetId ()){
						updateCheck = true;
						//se.updated = true;
						loaded[i] = se as T;
						SaveFile (loaded);
						
						//Debug.Log ("Serialized Updated [id = " + se.Id + "] " + se);
						break;
					}
				}
				if(!updateCheck){
					//Debug.Log ("Serialized Id not found!");
				}
			}
			
		}else{
			//Debug.Log ("No Serialized to save!");
		}
	}

	/*public T LoadByObjectId (string objectId) {
		T result = default (T);
		loaded = LoadFile ();
		
		if (loaded.Count > 0) {
			foreach(T t in loaded){
				if(t != null && t.ObjectId.Equals(objectId)) {
					result = t;
					//Debug.Log ("Serialized Loaded [ObjectId =  " + t.ObjectId + "] " + t + " from " + path);
					break;
				}
			}
		}else{
			//Debug.Log ("There are no Serializeds saved!");
		}

		if (result == default (T)) {
						//Debug.Log ("Not founded");
		}
		return result;
	}*/

	public T LoadById (int id) {
		T result = default (T);
		loaded = LoadFile ();

		if (loaded.Count > 0) {
			foreach(T t in loaded){
				if(t != null && t.GetId () == id) {
					result = t;
					//Debug.Log ("Serialized Loaded [id =  " + t.Id + "] " + t + " from " + path);
					break;
				}
			}
		}else{
			//Debug.Log ("There are no Serializeds saved!");
		}

		return result;
	}

	public List<T> LoadAll(){
		if (thread != null && thread.ThreadState == ThreadState.Running) {
			return loaded;
		}

		loaded = LoadFile ();

		if (loaded != null && loaded.Count > 0) {
			foreach(T t in loaded){
				if(t != null){
					//Debug.Log ("Serialized Loaded [id =  " + t.Id + "] " + t + " from " + path);
				}
				else
				{
					//Debug.Log ("Serialized is Deleted!");
				}
			}
		}else{
			//Debug.Log ("There are no Serializeds saved!");
		}

		return loaded;
	}

	public void Delete(DTO se){
		bool deleteCheck = false;
		loaded = LoadFile ();

		if(loaded.Count > 0){
			for(int i = 0; i < loaded.Count; i++){
				if(loaded[i] != null && loaded[i].GetId () == se.GetId ()){
					deleteCheck = true;
					loaded[i] = null;
					SaveFile (loaded);
					
					//Debug.Log ("Serialized Deleted [id = " + se.Id + "] ");
					break;
				}
			}
			if(!deleteCheck){
				//Debug.Log ("Serialized Id not found!");
			}
		}else{
			//Debug.Log ("There are no Serializeds to delete!");
		}
	}
	

	public void DeleteAll () {
		File.Delete (path);
		//Debug.Log ("Deleted all " + Type.GetType(typeof(T).ToString()).ToString () + "s");
	}

	public void DeleteFile () {
		if(File.Exists (path)){
			File.Delete (path);
		}
	}

	public virtual bool FileExists() {
		if (File.Exists (path))
			return true;
		else
			return false;
	}

	protected void SaveFile (List<T> ts){
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create (path);
		bf.Serialize(file, ts);
		file.Close();
	}

	protected List<T> LoadFile (){
		List<T> ts = new List<T> ();
		if(File.Exists (path)){
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(path, FileMode.Open);
			try {
				ts = (List<T>)bf.Deserialize(file);
			}catch (System.Exception e) {
				Debug.Log (e);
				DeleteAll ();
			}
			file.Close();
		}
		return ts;
	}
}
