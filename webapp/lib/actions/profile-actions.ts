"use server";

import {revalidatePath} from "next/cache";
import { fetchClient } from "../fetchClient";
import { Profile } from "../types";
import { EditProfileSchema } from "../schemas/editProfileSchema";

export async function getUserProfiles(sortBy?: string){
  let url = "/profiles";
  if (sortBy) url += "?sortBy=" + sortBy;
  return await fetchClient<Profile[]>(url, "GET");
}

export async function getProfileById(id: string){
  return await fetchClient<Profile>(`/profiles/${id}`, "GET");
}

export async function editProfile(id: string, profile: EditProfileSchema) {
  const result = await fetchClient<Profile>("/profiles/edit", "PUT", {body: profile});
  revalidatePath(`/profiles/${id}`);
  return result;
}