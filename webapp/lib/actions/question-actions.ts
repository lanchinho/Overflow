"use server";

import { fetchClient } from "../fetchClient";
import { Question } from "../types";

export async function getQuestions(tag?: string){
  let url = "/questions";    
  if (tag) url += "?tag=" + tag;  
  return fetchClient<Question[]>(url, "GET");
}

export async function getQuestionById(id: string){
  const url = `/questions/${id}`;     
  return fetchClient<Question>(url, "GET");
}

export async function searchQuestions(query: string){
  return fetchClient<Question[]>(`/search?query=${query}`, "GET");
}