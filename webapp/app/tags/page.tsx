import { getTags } from "@/lib/actions/tag-actions";
import TagCard from "./TagCard";
import TagPageHeader from "./TagsHeader";

type SearchParams = Promise<{sort?: string}>

export default async function Page({searchParams}: {searchParams: SearchParams}) {
  const {sort} = await searchParams;
  const {data: tags, error} = await getTags(sort);  
  if(error) throw error;

  return (
    <div className="w-full px-6">
      <TagPageHeader/>
      <div className="grid grid-cols-3 gap-4">
        {tags?.map(tag => (
          <TagCard tag={tag} key={tag.id}/>
        ))}
      </div>
    </div>
  );
}
