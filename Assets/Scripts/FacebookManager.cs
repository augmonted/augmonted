﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;

/*
 * This class is the wrapper for the Facebook API.
 * The single instance is use to call the API methods.
 * To call FB APIs simply do FacebookManager.Instance().MethodName()
 * 
 * DO NOT use Application.LoadLevel in here. Leave that up to
 * scripts local to the scene.
 */
public class FacebookManager : MonoBehaviour {

	static FacebookManager instance = null;

	public string FullName;
	public string Gender;
	public Sprite ProfilePic;
	
	private Dictionary<string, string> profile;
	
	string meQueryString = "/v2.0/me?fields=id,first_name,friends.limit(100).fields(first_name,id,picture.width(128).height(128)),invitable_friends.limit(100).fields(first_name,id,picture.width(128).height(128))";
	

	public static FacebookManager Instance() {
		return instance;
	}

	// Use this for initialization
	void Awake () {
		Debug.Log("FacebookManager: Awake");
		
		if (instance == null)
			instance = this;
		
		// Initialize FB SDK
		enabled = false;
		FB.Init (onInitCallback, onHideUnityCallback);
		DontDestroyOnLoad (gameObject);
	}

	public void callInit() {
		FB.Init (onInitCallback, onHideUnityCallback);
	}

	public void callLogin() {
		/*
		 * public_profile gives us access to these fields
		 * id
		 * name
		 * first_name
		 * last_name
		 * link
		 * gender
		 * locale
		 * timezone
		 * updated_time
		 * verified
		 */
		FB.Login ("public_profile, email", onLoginCallback);
	}

	public void callLogout() {
		FB.Logout ();
	}

	public bool IsLogged() {
		return FB.IsLoggedIn;
	}

	private void onInitCallback() {
		Debug.Log ("onInitCallback");
		
		enabled = true;
		Debug.Log("FB.IsLoggedIn value: " + FB.IsLoggedIn);
		if(FB.IsLoggedIn) {
			Debug.Log("Already logged in");
			onLoggedIn();
		}
		else {
			Debug.Log ("Not logged in");
		}
	}
	
	private void onLoggedIn() {
		Debug.Log("Logged in. ID: " + FB.UserId);
	
		// get profile picture
		FB.API (Util.GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, onPictureCallback);
		// get name
		FB.API ("/me?fields=id, name, first_name", Facebook.HttpMethod.GET, onNameCallback);
	}

	private void onHideUnityCallback(bool isGameShown) {
		if (!isGameShown)
			Time.timeScale = 0; // pause game
		else
			Time.timeScale = 1; // resume game
	}

	private void onLoginCallback(FBResult result) {
		if (FB.IsLoggedIn) {
			Debug.Log ("FB Login Worked");
			onLoggedIn();
		}
		else
			Debug.Log ("FB Login Failed");
	}

	private void onPictureCallback(FBResult result) {

		if (result.Error != null) {
			Debug.Log ("Could not get profile picture");
			
			// try again to get profile picture
			FB.API (Util.GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, onPictureCallback);
			return;
		} 

		ProfilePic = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2(0, 0));
		Debug.Log("Received profile picture");
	}

	private void onNameCallback(FBResult result) {
		// getting 400 bad request error each time
		if (result.Error != null) {
			Debug.Log ("Could not get a name");
			
			Debug.Log(result.Error);
			
			// try again to get name
			FB.API ("/me?fields=id, name, first_name", Facebook.HttpMethod.GET, onNameCallback);
			return;
		} 

		IDictionary dict = Facebook.MiniJSON.Json.Deserialize(result.Text) as IDictionary;

		profile = Util.DeserializeJSONProfile(result.Text);
		FullName = profile["first_name"];
		Debug.Log("Name is: " + FullName);
	}
}