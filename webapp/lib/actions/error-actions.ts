"use server";

import { fetchClient } from "../fetchClient";

export async function triggerError(code:number){
  return await fetchClient(`/test/errors?code=${code}`, "GET");
}