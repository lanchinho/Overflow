"use server";

import { fetchClient } from "../fetchClient";
import { Tag } from "../types";

export async function getTags(sort?: string){
  let url = "/tags";
  if(sort) url += "?sort=" + sort;
  
  return fetchClient<Tag[]>(url, "GET", {cache: "force-cache"});
}