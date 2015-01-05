using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class DTO {
	private int id;
	private bool updated = false;

	public int GetId() {
		return id;
	}
	
	public void SetId(int id) {
		this.id = id;
	}

	public bool IsUpdated () {
		return updated;
	}

	public void SetUpdated (bool updated) {
		this.updated = updated;
	}
}
