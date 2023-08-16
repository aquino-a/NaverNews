import React, { useEffect, useState } from 'react';

export const Articles = () => {
    const displayName = 'Articles';

    const [count, setCount] = useState(10);
    const [minimumCount, setMinimumCount] = useState(50);
    const [articles, setArticles] = useState([]);
    const [isLoading, setIsLoading] = useState(true);


    const loadArticles = async (olderThan, minimum, count) => {
        setIsLoading(true);
        const response = await fetch(`api/articles?olderThanUtc=${olderThan.toUTCString()}&minCount=${minimum}&count=${count}`);
        const data = await response.json();
        setArticles(data);
        setIsLoading(false);
    }

    useEffect(() => {
        loadArticles(new Date(), minimumCount, count);
    }, [articles, isLoading])

    if (isLoading) {
        return (<p><em>Loading...</em></p>);
    }

    const articleRows = articles.map(a => (
        <div id={a.ArticleId}>
            <img src={a.ImageUrl}></img>
            <label>{a.Title}</label>
            <br></br>
            <label>Comments : {a.CommentCount}</label>
            <label>Replies  : {a.ReplyCount}</label>
            <label>Total    : {a.Total}</label>
            <br></br>
            <article>{a.Summary}</article>
        </div>
    ))

    return (
        <div>
            {articleRows}
        </div>
    )
}
