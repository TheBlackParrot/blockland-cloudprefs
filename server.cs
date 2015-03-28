if(!isFile("config/server/cloud/prefs.cs")) {
	$Pref::CloudPreferences::Address = "example.com:80";
	$Pref::CloudPreferences::Directory = "/index.html";
	echo("\c2[Cloud] Be sure to set your web address in config/server/cloud/prefs.cs!");
	export("$Pref::CloudPreferences::*","config/server/cloud/prefs.cs");
} else {
	exec("config/server/cloud/prefs.cs");
}

if(!isObject(CloudPreferencesHTTP)) {
	new HTTPObject(CloudPreferencesHTTP);
}
$CloudPreferences::RequiredAddon = "";
if($Pref::CloudPreferences::Address !$= "example.com:80") {
	CloudPreferencesHTTP.get($Pref::CloudPreferences::Address,$Pref::CloudPreferences::Directory);
}


function CloudPreferencesHTTP::onLine(%this,%line) {
	if(getSubStr(%line,0,2) $= "//") {
		return;
	}
	if(getSubStr(%line,0,5) $= "ADDON") {
		%addon = getWord(%line,1);
		if(findFirstFile("Add-Ons/" @ %addon @ "/server.cs") !$= "") {
			echo("\c4[Cloud] Loading" SPC %addon);
			exec("Add-Ons/" @ %addon @ "/server.cs");
		} else {
			echo("\c2[Cloud] ERROR: Requested addon" SPC %addon SPC "does not exist!");
		}
		return;
	}
	if(getSubStr(%line,0,2) $= "+!") {
		if($CloudPreferences::RequiredAddon !$= "") {
			echo("\c2[Cloud] ERROR:" SPC $CloudPreferences::RequiredAddon SPC "is already set as a required addon!");
			return;
		}
		echo("\c1[Cloud] Requirement for" SPC getWord(%line,1) SPC "was set.");
		$CloudPreferences::RequiredAddon = getWord(%line,1);
		return;
	}
	if(getSubStr(%line,0,2) $= "-!") {
		%req = getWord(%line,1);
		if(%req !$= $CloudPreferences::RequiredAddon) {
			echo("\c2[Cloud] ERROR: Requirement mismatch on line \"" @ %line @ "\" -- was looking for" SPC $CloudPreferences::RequiredAddon @ "!");
			return;
		} else {
			$CloudPreferences::RequiredAddon = "";
			echo("\c1[Cloud] Requirement for" SPC %req SPC "removed.");
		}
		return;
	}

	%var_name = stripMLControlChars(getField(%line,0));
	%var_value = getField(%line,1);
	%stripml_value = stripMLControlChars(getField(%line,2));

	if($CloudPreferences::RequiredAddon !$= "") {
		if(findFirstFile("Add-Ons/" @ $CloudPreferences::RequiredAddon @ "/*") $= "") {
			echo("\c3[Cloud] Skipping" SPC %var_name @ ", required addon not present...");
			return;
		}
	}

	// trust your sources
	if(%stripml_value) {
		%var_value = stripMLControlChars(%var_value);
	}

	%eval_str = %var_name @ "=\"" @ %var_value @ "\"; echo(\"\\c4[Cloud] Set " @ %var_name @ " to " @ %var_value @ "\");";
	eval(%eval_str);
}