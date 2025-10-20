"use server";

import { fetchClient } from "../fetchClient";
import { Tag } from "../types";

export async function getTags(){
  return fetchClient<Tag[]>("/tags", "GET", {cache: "force-cache"});
}